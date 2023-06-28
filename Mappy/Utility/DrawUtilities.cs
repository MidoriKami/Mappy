using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using ImGuiScene;
using KamiLib;
using KamiLib.Caching;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;
using Mappy.System;
using Mappy.Views.Windows;

namespace Mappy.Utility;

public class DrawUtilities
{
    public static void DrawIcon(TextureWrap? iconTexture, Vector2 position, float scale = 0.5f)
    {
        if (iconTexture is null) return;
        if (MappySystem.MapTextureController is not { Ready: true }) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { } mapWindow) return;
        
        var iconSize = new Vector2(iconTexture.Width, iconTexture.Height) * scale;
        mapWindow.Viewport.SetImGuiDrawPosition(position * mapWindow.Viewport.Scale - iconSize / 2.0f);
        ImGui.Image(iconTexture.ImGuiHandle, iconSize);
    }
    
    public static void DrawIcon(uint iconId, Vector2 position, float scale = 0.50f) 
        => DrawIcon(IconCache.Instance.GetIcon(iconId), position, scale);

    public static void DrawIcon(uint iconId, GameObject gameObject, Map map, float scale = 0.50f)
    {
        var icon = IconCache.Instance.GetIcon(iconId);
        var position = Position.GetObjectPosition(gameObject, map);
        
        DrawIcon(icon, position, scale);
    }

    private static void DrawLevelRing(Level level, Viewport viewport, Map map, Vector4 color, float extraRadius = 0.0f)
    {
        var position = Position.GetTextureOffsetPosition(new Vector2(level.X, level.Z), map);
        var drawPosition = viewport.GetImGuiWindowDrawPosition(position);
        var radius = level.Radius * viewport.Scale / 7.0f + extraRadius * viewport.Scale;
        
        var imGuiColor = ImGui.GetColorU32(color);
        
        ImGui.BeginGroup();
        ImGui.GetWindowDrawList().AddCircleFilled(drawPosition, radius, imGuiColor);
        ImGui.GetWindowDrawList().AddCircle(drawPosition, radius, imGuiColor, 0, 4);
        ImGui.EndGroup();
    }
    
    public static void DrawLevelObjective(Level level, uint iconId, string label, Vector4 ringColor, Vector4 tooltipColor, Viewport viewport, Map map, bool showTooltip, float scale = 0.50f, float extraRadius = 0.0f)
    {
        DrawLevelRing(level, viewport, map, ringColor, extraRadius);
        DrawLevelIcon(level, iconId, map, scale);
        if(showTooltip) DrawTooltip(label, tooltipColor, iconId);
    }

    private static void DrawLevelIcon(Level level, uint iconId, Map map, float scale = 0.50f)
    {
        var position = Position.GetTextureOffsetPosition(new Vector2(level.X, level.Z), map);

        DrawIcon(iconId, position, scale);
    }
    
    public static void DrawImageRotated(TextureWrap? texture, GameObject gameObject, float iconScale = 0.5f)
    {
        if (texture is null) return;
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { } mapWindow) return;
        
        var objectPosition = Position.GetObjectPosition(gameObject.Position, map);
        var center = mapWindow.Viewport.GetImGuiWindowDrawPosition(objectPosition);
        var angle = GetObjectRotation(gameObject);
        
        var size = new Vector2(texture.Width, texture.Height) * iconScale;

        var cosA = MathF.Cos(angle + 0.5f * MathF.PI);
        var sinA = MathF.Sin(angle + 0.5f * MathF.PI);

        Vector2[] vectors =
        {
            center + ImRotate(new Vector2(-size.X * 0.5f, -size.Y * 0.5f), cosA, sinA),
            center + ImRotate(new Vector2(+size.X * 0.5f, -size.Y * 0.5f), cosA, sinA),
            center + ImRotate(new Vector2(+size.X * 0.5f, +size.Y * 0.5f), cosA, sinA),
            center + ImRotate(new Vector2(-size.X * 0.5f, +size.Y * 0.5f), cosA, sinA)
        };

        var windowDrawList = ImGui.GetWindowDrawList();
        windowDrawList.AddImageQuad(texture.ImGuiHandle, vectors[0], vectors[1], vectors[2], vectors[3]);
    }
    
    public static void DrawTooltipIcon(uint mapIcon)
    {
        if (IconCache.Instance.GetIcon(mapIcon) is { } icon)
        {
            ImGui.Image(icon.ImGuiHandle, new Vector2(24.0f));

            ImGui.SameLine();
            var cursorPosition = ImGui.GetCursorPos();
            const float offset = 3.0f;
            ImGui.SetCursorPos(cursorPosition with { Y = cursorPosition.Y + offset });
        }
    }
    
    public static void DrawTooltip(string text, Vector4 color)
    {
        if (!ImGui.IsItemHovered()) return;
        
        ImGui.BeginTooltip();
        ImGui.TextColored(color, text);
        ImGui.EndTooltip();
    }

    public static void DrawTooltip(string text, Vector4 color, uint iconId, uint? secondIconId = null)
    {
        if (!ImGui.IsItemHovered()) return;
        ImGui.BeginTooltip();

        DrawTooltipIcon(iconId);
        if (secondIconId is not null) DrawTooltipIcon(secondIconId.Value);

        ImGui.TextColored(color, text);
        ImGui.EndTooltip();
    }

    public static void DrawMultiTooltip(string primaryText, string extraText, Vector4 color, uint iconId)
    {
        if (!ImGui.IsItemHovered()) return;

        ImGui.BeginTooltip();
        
        DrawTooltipIcon(iconId);
        
        var cursorPos = ImGui.GetCursorPos();
        ImGui.TextColored(color, primaryText);
        ImGui.SameLine();
        ImGui.SetCursorPos(cursorPos with { Y = ImGui.GetCursorPos().Y + 5.0f } );
        ImGui.TextColored(color with { W = 0.45f }, $"\n{extraText}");
        ImGui.EndTooltip();
    }

    private static float GetObjectRotation(GameObject gameObject) 
        => -gameObject.Rotation + 0.5f * MathF.PI;

    private static Vector2 ImRotate(Vector2 v, float cosA, float sinA) 
        => new(v.X * cosA - v.Y * sinA, v.X * sinA + v.Y * cosA);
}