using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using KamiLib.Caching;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;
using Mappy.System;

namespace Mappy.Utility;

public partial class DrawUtilities
{
    private static IconLayer? GetDirectionalIconLayer(MappyMapIcon iconData)
    {
        var offsetPosition = new Vector2(8.0f, 24.0f);
        
        var isBelowPlayer = false;
        var isAbovePlayer = false;

        if (Service.ClientState is { LocalPlayer.Position.Y: var playerHeight })
        {
            var distance = playerHeight - iconData.VerticalPosition;

            if (Math.Abs(distance) > iconData.VerticalThreshold)
            {
                isBelowPlayer = distance > 0;
                isAbovePlayer = distance < 0;
            }
        }

        if (isBelowPlayer)
        {
            return new IconLayer(60545, offsetPosition);
        }
        else if (isAbovePlayer)
        {
            return new IconLayer(60541, offsetPosition);
        }
        else
        {
            return null;
        }
    }
    
    private static bool IsIconDisabled(uint iconId)
    {
        if (MappySystem.SystemConfig is not { SeenIcons: var seenIcons, DisallowedIcons: var disallowedIcons }) return true;

        if (!seenIcons.Contains(iconId) && iconId is not 0)
        {
            seenIcons.Add(iconId);
            MappyPlugin.System.SaveConfig();
        }

        return disallowedIcons.Contains(iconId);
    }
    
    private static void DrawIconTexture(TextureWrap? iconTexture, Viewport viewport, Vector2 position, float scale)
    {
        if (iconTexture is null) return;
        if (MappySystem.MapTextureController is not { Ready: true }) return;
        
        var iconSize = new Vector2(iconTexture.Width, iconTexture.Height) * scale;
        
        viewport.SetImGuiDrawPosition(position * viewport.Scale - iconSize / 2.0f);
        ImGui.Image(iconTexture.ImGuiHandle, iconSize);
    }
    
    private static float GetObjectRotation(GameObject gameObject) 
        => -gameObject.Rotation + 0.5f * MathF.PI;
    
    private static Vector2 ImRotate(Vector2 v, float cosA, float sinA) 
        => new(v.X * cosA - v.Y * sinA, v.X * sinA + v.Y * cosA);
    
    private static Vector2[] GetRotationVectors(float angle, Vector2 center, Vector2 size)
    {
        var cosA = MathF.Cos(angle + 0.5f * MathF.PI);
        var sinA = MathF.Sin(angle + 0.5f * MathF.PI);
    
        Vector2[] vectors =
        {
            center + ImRotate(new Vector2(-size.X * 0.5f, -size.Y * 0.5f), cosA, sinA),
            center + ImRotate(new Vector2(+size.X * 0.5f, -size.Y * 0.5f), cosA, sinA),
            center + ImRotate(new Vector2(+size.X * 0.5f, +size.Y * 0.5f), cosA, sinA),
            center + ImRotate(new Vector2(-size.X * 0.5f, +size.Y * 0.5f), cosA, sinA)
        };
        return vectors;
    }

    private static float GetLevelRingRadius(float radius, Viewport viewport, Map map)
        => radius * viewport.Scale * map.SizeFactor / 100.0f;
    
    private static void DrawAreaRing(Vector2 position, float radius, Viewport viewport, Map map, Vector4 color)
    {
        var drawPosition = viewport.GetImGuiWindowDrawPosition(position);
        var calculatedRadius = GetLevelRingRadius(radius, viewport, map);
        var imGuiColor = ImGui.GetColorU32(color);
        
        ImGui.GetWindowDrawList().AddCircleFilled(drawPosition, calculatedRadius, imGuiColor);
        ImGui.GetWindowDrawList().AddCircle(drawPosition, calculatedRadius, imGuiColor, 0, 4);
    }
    
    private static void DrawTooltipIcon(uint iconId)
    {
        if (IconCache.Instance.GetIcon(iconId) is not { } icon) return;
        
        ImGui.Image(icon.ImGuiHandle, new Vector2(24.0f, 24.0f));
        ImGui.SameLine();
        ImGui.SetCursorPos(ImGui.GetCursorPos() with { Y = ImGui.GetCursorPos().Y + 3.0f } );
    }

    private static void DrawStandardTooltipInternal(uint iconId, uint secondIconId, Vector4 color, string primaryText, string secondaryText) 
        => DrawTooltipInternal(iconId, secondIconId, color, primaryText, secondaryText);

    private static void DrawTooltip(uint iconId, uint secondIconId, Vector4 color, string primaryText, string secondaryText = "")
        => DrawStandardTooltipInternal(iconId, secondIconId, color, primaryText, secondaryText);
    
    private static void DrawAreaTooltip(Vector2 position, float radius, Viewport viewport, uint iconId, Vector4 color, string primaryText, string secondaryText = "")
        => DrawAreaTooltipInternal(position, radius * viewport.Scale, viewport, iconId, 0, color, primaryText, secondaryText);
    
    private static void DrawAreaTooltipInternal(Vector2 position, float radius, Viewport viewport, uint iconId, uint secondIconId, Vector4 color, string primaryText, string secondaryText)
    {
        if (!Bound.IsCursorInWindow()) return;
        
        iconId = TryReplaceIconId(iconId);
        var levelLocation = position * viewport.Scale + viewport.StartPosition - viewport.Offset;
        var cursorLocation = ImGui.GetMousePos();

        if (Vector2.Distance(levelLocation, cursorLocation) * viewport.Scale > radius * viewport.Scale) return;
        DrawTooltipInternal(iconId, secondIconId, color, primaryText, secondaryText);
    }
    
    private static void DrawTooltipInternal(uint iconId, uint secondIconId, Vector4 color, string primaryText, string secondaryText)
    {
        if (ImGui.IsPopupOpen(string.Empty, ImGuiPopupFlags.AnyPopup)) return;
        if (primaryText.IsNullOrEmpty() && secondaryText.IsNullOrEmpty()) return;
        
        ImGui.BeginTooltip();
        
        if(iconId is not 0) DrawTooltipIcon(iconId);
        if (secondIconId is not 0) DrawTooltipIcon(secondIconId);

        var cursorPosition = ImGui.GetCursorPos();
        
        if(!primaryText.IsNullOrEmpty()) ImGui.TextColored(color, primaryText);
        if (!secondaryText.IsNullOrEmpty())
        {
            ImGui.SameLine();
            ImGui.SetCursorPos(cursorPosition with { Y = ImGui.GetCursorPos().Y + 5.0f } );
            ImGui.TextColored(KnownColor.Gray.AsVector4(), $"\n{secondaryText}");
        }
        
        ImGui.EndTooltip();
    }

    private static uint TryReplaceIconId(uint iconId) => iconId switch
    {
        >= 60483 and <= 60494 => 60071,
        _ => iconId,
    };
}