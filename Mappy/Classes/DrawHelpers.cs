using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using KamiLib.Classes;
using Mappy.Data;

namespace Mappy.Classes;

public class MarkerInfo {
    public required Vector2 Position { get; set; }
    public required Vector2 Offset { get; set; }
    public required float Scale { get; init; }
    public uint? ObjectiveId { get; init; }
    public uint? LevelId { get; set; }
    public uint IconId { get; set; }
    public Func<string?>? PrimaryText { get; set; }
    public Func<string?>? SecondaryText { get; set; }
    public float? Radius { get; set; }
    public Vector4 RadiusColor { get; set; } = KnownColor.CornflowerBlue.Vector();
    public Action? OnRightClicked { get; set; }
    public Action? OnLeftClicked { get; set; }
    public bool IsDynamicMarker { get; init; }
}

public static class DrawHelpers {
    public static Vector2 GetMapOffsetVector() 
        => GetRawMapOffsetVector() * GetMapScaleFactor();

    public static unsafe Vector2 GetRawMapOffsetVector() 
        => new(AgentMap.Instance()->SelectedOffsetX, AgentMap.Instance()->SelectedOffsetY);
    
    public static unsafe float GetMapScaleFactor()
        => AgentMap.Instance()->SelectedMapSizeFactorFloat;

    public static Vector2 GetMapCenterOffsetVector() 
        => new(1024.0f, 1024.0f);

    public static Vector2 GetCombinedOffsetVector()
        => -GetMapOffsetVector() + GetMapCenterOffsetVector();

    public static void DrawMapMarker(MarkerInfo markerInfo) {
        if (markerInfo.IconId is 0) return;
        
        // Don't draw markers that are positioned off the map texture
        if (markerInfo.Position.X < 0.0f || markerInfo.Position.X > 2048.0f * markerInfo.Scale || markerInfo.Position.Y < 0.0f || markerInfo.Position.Y > 2048.0f * markerInfo.Scale) return;

        // Translate circle markers that don't have icons, into [?] icon
        if (markerInfo.IconId is >= 60483 and <= 60494) {
            markerInfo.IconId = 60071;
        }

        // If this is the first time we have seen this iconId, save it
        if (System.IconConfig.IconSettingMap.TryAdd(markerInfo.IconId, new IconSetting {
                IconId = markerInfo.IconId,
            })) {
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

    private static void DrawRadiusUnderlay(MarkerInfo markerInfo) {
        if (markerInfo is not { Radius: { } markerRadius and > 1.0f }) return;
        
        var center = markerInfo.Position + markerInfo.Offset + ImGui.GetWindowPos();
        var radius = markerRadius * markerInfo.Scale;
        var fillColor = ImGui.GetColorU32(markerInfo.RadiusColor with { W = 0.33f });
        var radiusColor =  ImGui.GetColorU32(markerInfo.RadiusColor);
            
        ImGui.GetWindowDrawList().AddCircleFilled(center, radius, fillColor);
        ImGui.GetWindowDrawList().AddCircle(center, radius, radiusColor, 0, 3.0f);
    }
    
    private static void DrawIcon(MarkerInfo markerInfo) {
        var texture = Service.TextureProvider.GetFromGameIcon(markerInfo.IconId).GetWrapOrEmpty();
        
        ImGui.SetCursorPos(markerInfo.Position + markerInfo.Offset - texture.Size * System.SystemConfig.IconScale / 2.0f * markerInfo.Scale * System.IconConfig.IconSettingMap[markerInfo.IconId].Scale);
        ImGui.Image(texture.ImGuiHandle, texture.Size * markerInfo.Scale * System.SystemConfig.IconScale * System.IconConfig.IconSettingMap[markerInfo.IconId].Scale);
    }
    
    private static void ProcessInteractions(MarkerInfo markerInfo) {
        if (System.IconConfig.IconSettingMap[markerInfo.IconId] is not { AllowClick: true }) return;
        
        if (markerInfo is { OnRightClicked: { } rightClickAction } && ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
            rightClickAction.Invoke();
        }

        if (markerInfo is { OnLeftClicked: { } leftClickAction } && ImGui.IsItemClicked(ImGuiMouseButton.Left)) {
            leftClickAction.Invoke();
        }
    }
    
    private static void DrawTooltip(MarkerInfo markerInfo) {
        if (System.IconConfig.IconSettingMap[markerInfo.IconId] is { AllowTooltip: false }) {
            return;
        }
        
        var isActivatedViaRadius = false;

        if (markerInfo is { Radius: { } sameRadius and > 1.0f }) {
            var center = markerInfo.Position + markerInfo.Offset + ImGui.GetWindowPos();
            var radius = sameRadius * markerInfo.Scale;

            if (Vector2.Distance(ImGui.GetMousePos() - System.MapWindow.MapDrawOffset + ImGui.GetWindowPos(), center) <= radius && System.MapWindow.IsMapHovered) {
                isActivatedViaRadius = true;
            }
        }
        
        if (isActivatedViaRadius || ImGui.IsItemHovered()) {
            if (markerInfo.PrimaryText?.Invoke() is { Length: > 0 } primaryText) {
                using var tooltip = ImRaii.Tooltip();
                
                ImGui.Image(Service.TextureProvider.GetFromGameIcon(markerInfo.IconId).GetWrapOrEmpty().ImGuiHandle, ImGuiHelpers.ScaledVector2(32.0f, 32.0f));
                    
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
}