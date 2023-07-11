using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using KamiLib.Caching;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;

namespace Mappy.Utility;

public partial class DrawUtilities
{
    private static float GetRingScale(Map map) => map.SizeFactor switch
    {
        100 => 4.0f,
        200 => 1.60f,
        _ => 1.0f,
    };
    
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

    private static float GetLevelRingRadius(Level level, Viewport viewport, Map map, float extraRadius)
        => level.Radius * viewport.Scale / GetRingScale(map) + extraRadius * viewport.Scale;

    private static void DrawLevelRing(Level level, Viewport viewport, Map map, Vector4 color, float extraRadius)
    {
        var position = Position.GetTextureOffsetPosition(new Vector2(level.X, level.Z), map);
        var drawPosition = viewport.GetImGuiWindowDrawPosition(position);
        var radius = GetLevelRingRadius(level, viewport, map, extraRadius);
        var imGuiColor = ImGui.GetColorU32(color);
        
        ImGui.GetWindowDrawList().AddCircleFilled(drawPosition, radius, imGuiColor);
        ImGui.GetWindowDrawList().AddCircle(drawPosition, radius, imGuiColor, 0, 4);
    }
    
    private static void DrawLevelIcon(Level level, uint iconId, Map map, float scale)
    {
        var position = Position.GetTextureOffsetPosition(new Vector2(level.X, level.Z), map);

        DrawIcon(iconId, position, scale);
    }
    
    private static void DrawTooltipIcon(uint iconId)
    {
        if (IconCache.Instance.GetIcon(iconId) is not { } icon) return;
        
        ImGui.Image(icon.ImGuiHandle, new Vector2(24.0f, 24.0f));
        ImGui.SameLine();
        ImGui.SetCursorPos(ImGui.GetCursorPos() with { Y = ImGui.GetCursorPos().Y + 3.0f } );
    }

    private static void DrawStandardTooltipInternal(uint iconId, uint secondIconId, Vector4 color, string primaryText, string secondaryText)
    {
        if (!ImGui.IsItemHovered()) return;
        DrawTooltipInternal(iconId, secondIconId, color, primaryText, secondaryText);
    }

    // Note: LevelTooltips aren't allowed to have double icons
    private static void DrawLevelTooltipInternal(Level level, Viewport viewport, Map map, float extraRadius, uint iconId, Vector4 color, string primaryText, string secondaryText)
    {
        var radius = GetLevelRingRadius(level, viewport, map, extraRadius);
        
        DrawLevelTooltipInternal(new Vector2(level.X, level.Z), radius, viewport, map, iconId, color, primaryText, secondaryText);
    }

    private static void DrawLevelTooltipInternal(Vector2 position, float radius, Viewport viewport, Map map, uint iconId, Vector4 color, string primaryText, string secondaryText)
    {
        iconId = iconId is 60492 or 60491 ? 060071 : iconId; // Replace nonexistent markers with our custom ? marker
        var levelTextureLocation = Position.GetTextureOffsetPosition(position, map);
        var levelLocation = levelTextureLocation * viewport.Scale + viewport.StartPosition - viewport.Offset;
        var cursorLocation = ImGui.GetMousePos();

        if (Vector2.Distance(levelLocation, cursorLocation) * viewport.Scale > radius * viewport.Scale) return;
        DrawTooltipInternal(iconId, 0, color, primaryText, secondaryText);
    }
    
    private static void DrawTooltipInternal(uint iconId, uint secondIconId, Vector4 color, string primaryText, string secondaryText)
    {
        ImGui.BeginTooltip();
        
        if(iconId is not 0) DrawTooltipIcon(iconId);
        if (secondIconId is not 0) DrawTooltipIcon(secondIconId);

        var cursorPosition = ImGui.GetCursorPos();
        
        if(primaryText != string.Empty) ImGui.TextColored(color, primaryText);
        if (secondaryText != string.Empty)
        {
            ImGui.SameLine();
            ImGui.SetCursorPos(cursorPosition with { Y = ImGui.GetCursorPos().Y + 5.0f } );
            ImGui.TextColored(color with { W = 0.45f }, $"\n{secondaryText}");
        }
        
        ImGui.EndTooltip();
    }
}