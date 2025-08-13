using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.Classes;
using Mappy.Data;
using SeString = Dalamud.Game.Text.SeStringHandling.SeString;

namespace Mappy.Classes;

public class MarkerInfo
{
    public required Vector2 Position { get; set; }
    public required Vector2 Offset { get; set; }
    public required float Scale { get; set; }
    public uint? ObjectiveId { get; init; }
    public uint? DataId { get; set; }
    public MarkerType MarkerType { get; set; }
    public uint IconId { get; set; }
    public Func<string?>? PrimaryText { get; set; }
    public Func<string?>? SecondaryText { get; set; }
    public float? Radius { get; set; }
    public Vector4 RadiusColor { get; set; } = KnownColor.CornflowerBlue.Vector();
    public Vector4 RadiusOutlineColor { get; set; } = KnownColor.CornflowerBlue.Vector();
    public Action? OnRightClicked { get; set; }
    public Action? OnLeftClicked { get; set; }
    public bool IsDynamicMarker { get; init; }
}

public static class DrawHelpers
{
    private static bool DebugMode => System.SystemConfig.DebugMode;

    public const uint QuestionMarkIcon = 60071;

    /// <summary>
    /// Offset Vector of SelectedX, SelectedY, scaled with SelectedSizeFactor
    /// </summary>
    public static Vector2 GetMapOffsetVector() => GetRawMapOffsetVector() * GetMapScaleFactor();

    /// <summary>
    /// Unscaled Vector of SelectedX, SelectedY
    /// </summary>
    public static unsafe Vector2 GetRawMapOffsetVector() => new(AgentMap.Instance()->SelectedOffsetX, AgentMap.Instance()->SelectedOffsetY);

    /// <summary>
    /// Selected Scale Factor
    /// </summary>
    public static unsafe float GetMapScaleFactor() => AgentMap.Instance()->SelectedMapSizeFactorFloat;

    /// <summary>
    /// 1024 vector, center offset vector
    /// </summary>
    public static Vector2 GetMapCenterOffsetVector() => new(1024.0f, 1024.0f);

    /// <summary>
    /// Offset for the top left corner of the drawn map
    /// </summary>
    public static Vector2 GetCombinedOffsetVector() => -GetMapOffsetVector() + GetMapCenterOffsetVector();

    public static void DrawMapMarker(MarkerInfo markerInfo)
    {
        if (markerInfo.IconId is 0) return;

        // Don't draw markers that are positioned off the map texture
        if (markerInfo.Position.X < 0.0f || markerInfo.Position.X > 2048.0f * markerInfo.Scale || markerInfo.Position.Y < 0.0f ||
            markerInfo.Position.Y > 2048.0f * markerInfo.Scale)
            return;

        markerInfo.IconId = markerInfo.IconId switch
        {
            // Translate circle markers that don't have icons, into [?] icon
            >= 60483 and <= 60494 => QuestionMarkIcon,

            // Translate Gemstone Trader Icon into smaller version... why square, why.
            60091 => 61731,

            // Leave all other icons as they were
            _ => markerInfo.IconId,
        };

        if (DebugMode) {
            markerInfo.SecondaryText = markerInfo.PrimaryText;
            markerInfo.PrimaryText = () => $"[Debug] IconId: {markerInfo.IconId}";
        }

        // If this is the first time we have seen this iconId, save it
        if (System.IconConfig.IconSettingMap.TryAdd(markerInfo.IconId, new IconSetting { IconId = markerInfo.IconId, })) {
            System.IconConfig.Save();
        }

        // If this icon is disabled, don't even process it
        if (System.IconConfig.IconSettingMap[markerInfo.IconId] is { Hide: true }) {
            return;
        }

        // Only process modules for Dynamic Markers
        if (markerInfo.IsDynamicMarker) {
            foreach (var module in System.Modules) {
                if (module.ProcessMarker(markerInfo)) {
                    break;
                }
            }
        }

        DrawRadiusUnderlay(markerInfo);
        DrawIcon(markerInfo);
        ProcessInteractions(markerInfo);
        DrawTooltip(markerInfo);
    }

    private static unsafe void DrawRadiusUnderlay(MarkerInfo markerInfo)
    {
        if (markerInfo is not { Radius: { } markerRadius and > 1.0f }) return;

        var center = markerInfo.Position + markerInfo.Offset + ImGui.GetWindowPos();
        var radius = markerRadius * markerInfo.Scale * AgentMap.Instance()->SelectedMapSizeFactorFloat;
        var fillColor = ImGui.GetColorU32(markerInfo.RadiusColor with { W = System.SystemConfig.AreaColor.W });
        var radiusColor = ImGui.GetColorU32(markerInfo.RadiusOutlineColor with { W = System.SystemConfig.AreaOutlineColor.W });

        ImGui.GetWindowDrawList().AddCircleFilled(center, radius, fillColor);
        ImGui.GetWindowDrawList().AddCircle(center, radius, radiusColor, 0, 3.0f);
    }

    private static void DrawIcon(MarkerInfo markerInfo)
    {
        var texture = Service.TextureProvider.GetFromGameIcon(markerInfo.IconId).GetWrapOrEmpty();
        var scale = System.SystemConfig.ScaleWithZoom ? markerInfo.Scale : 1.0f;

        var iconScale = System.SystemConfig.IconScale;

        if (markerInfo.IconId is 60401 or 60402) {
            scale *= 2.0f;
        }

        // Fixed scale not supported for map region markers
        if (IsRegionIcon(markerInfo.IconId)) {
            scale = markerInfo.Scale;
            iconScale = 0.42f;
        }

        ImGui.SetCursorPos(markerInfo.Position + markerInfo.Offset - texture.Size * iconScale / 2.0f * scale * System.IconConfig.IconSettingMap[markerInfo.IconId].Scale);
        var cursorScreenPos = ImGui.GetCursorScreenPos();
        var iconSize = texture.Size * scale * iconScale * System.IconConfig.IconSettingMap[markerInfo.IconId].Scale;

        ImGui.Image(texture.Handle, iconSize, Vector2.Zero, Vector2.One, System.IconConfig.IconSettingMap[markerInfo.IconId].Color);

        if (DebugMode) {
            foreach (var x in Enumerable.Range(-1, 3)) {
                foreach (var y in Enumerable.Range(-1, 3)) {
                    ImGui.GetWindowDrawList().AddRect(cursorScreenPos + new Vector2(x, y), cursorScreenPos + iconSize, ImGui.GetColorU32(KnownColor.White.Vector()), 3.0f);
                }
            }

            ImGui.GetWindowDrawList().AddRect(cursorScreenPos, cursorScreenPos + iconSize, ImGui.GetColorU32(KnownColor.Red.Vector()), 3.0f);
        }
    }

    public static void DrawText(MarkerInfo markerInfo, SeString text) => DrawText(markerInfo, text.ToString());

    public static void DrawText(MarkerInfo markerInfo, string text)
    {
        using var largeFont = System.LargeAxisFontHandle.Push();
        ImGui.SetWindowFontScale(markerInfo.Scale);

        var textSize = ImGui.CalcTextSize(text);
        var drawPosition = markerInfo.Position + markerInfo.Offset + ImGui.GetWindowPos() - textSize / 2.0f;

        drawPosition = new Vector2(MathF.Round(drawPosition.X), MathF.Round(drawPosition.Y));

        if (System.SystemConfig.DebugMode) {
            ImGui.GetWindowDrawList().AddCircleFilled(markerInfo.Position + markerInfo.Offset + ImGui.GetWindowPos(), 5.0f, ImGui.GetColorU32(KnownColor.Red.Vector()));
            ImGui.GetWindowDrawList().AddRect(drawPosition, drawPosition + textSize, ImGui.GetColorU32(KnownColor.Green.Vector()), 3.0f);
        }

        foreach (var x in Enumerable.Range(-1, 3)) {
            foreach (var y in Enumerable.Range(-1, 3)) {
                if (x is 0 && y is 0) continue;

                ImGui.SetCursorScreenPos(drawPosition + new Vector2(x, y));
                ImGui.TextColored(KnownColor.Black.Vector(), text);
            }
        }

        ImGui.SetCursorScreenPos(drawPosition);
        ImGui.TextColored(KnownColor.White.Vector(), text);

        ImGui.SetWindowFontScale(1.0f);
    }

    private static void ProcessInteractions(MarkerInfo markerInfo)
    {
        if (System.IconConfig.IconSettingMap[markerInfo.IconId] is not { AllowClick: true }) return;

        if (markerInfo is { OnRightClicked: { } rightClickAction } && ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
            rightClickAction.Invoke();
        }

        if (markerInfo is { OnLeftClicked: { } leftClickAction } && ImGui.IsItemClicked(ImGuiMouseButton.Left)) {
            leftClickAction.Invoke();
        }
    }

    private static unsafe void DrawTooltip(MarkerInfo markerInfo)
    {
        if (System.IconConfig.IconSettingMap[markerInfo.IconId] is { AllowTooltip: false } && !DebugMode) {
            return;
        }

        var isActivatedViaRadius = false;

        if (markerInfo is { Radius: { } sameRadius and > 1.0f }) {
            var center = markerInfo.Position + markerInfo.Offset + ImGui.GetWindowPos();
            var radius = sameRadius * markerInfo.Scale * AgentMap.Instance()->SelectedMapSizeFactorFloat;

            if (Vector2.Distance(ImGui.GetMousePos() - System.MapWindow.MapDrawOffset + ImGui.GetWindowPos(), center) <= radius && System.MapWindow.HoveredFlags.Any()) {
                isActivatedViaRadius = true;
            }
        }

        if (isActivatedViaRadius || ImGui.IsItemHovered()) {
            if (markerInfo.PrimaryText?.Invoke() is { Length: > 0 } primaryText) {
                using var tooltip = ImRaii.Tooltip();

                ImGui.Image(Service.TextureProvider.GetFromGameIcon(markerInfo.IconId).GetWrapOrEmpty().Handle, ImGuiHelpers.ScaledVector2(32.0f, 32.0f));

                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 7.5f * ImGuiHelpers.GlobalScale);
                var cursorPosition = ImGui.GetCursorPos();
                ImGui.Text(primaryText);

                if (markerInfo.SecondaryText?.Invoke() is { Length: > 0 } secondaryText) {
                    ImGui.SameLine();
                    ImGui.SetCursorPos(cursorPosition);
                    ImGuiTweaks.TextColoredUnformatted(KnownColor.Gray.Vector(), $"\n{secondaryText}");
                }
            }
        }
    }

    public static bool IsDisallowedIcon(uint iconId) =>
        iconId switch
        {
            60091 => true,
            _ when IsRegionIcon(iconId) => true,
            _ => false,
        };

    public static bool IsRegionIcon(uint iconId) =>
        iconId switch
        {
            >= 63200 and < 63900 => true,
            >= 62620 and < 62800 => true,
            _ => false,
        };
}