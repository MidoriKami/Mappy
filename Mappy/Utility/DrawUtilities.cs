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

public partial class DrawUtilities
{
    public static void DrawGameObjectIcon(uint iconId, GameObject gameObject, Map map, float scale)
    {
        var objectPosition = Position.GetObjectPosition(gameObject, map);
        
        DrawIcon(iconId, objectPosition, scale);
    }
    
    public static void DrawIcon(uint iconId, Vector2 position, float scale)
        => DrawIconTexture(IconCache.Instance.GetIcon(iconId), position, scale);
    
    public static void DrawIconTexture(TextureWrap? iconTexture, Vector2 position, float scale)
    {
        if (iconTexture is null) return;
        if (MappySystem.MapTextureController is not { Ready: true }) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { } mapWindow) return;
        
        var iconSize = new Vector2(iconTexture.Width, iconTexture.Height) * scale;
        
        mapWindow.Viewport.SetImGuiDrawPosition(position * mapWindow.Viewport.Scale - iconSize / 2.0f);
        ImGui.Image(iconTexture.ImGuiHandle, iconSize);
    }

    public static void DrawTooltip(Vector4 color, string primaryText)
        => DrawStandardTooltipInternal(0, 0, color, primaryText, string.Empty);

    public static void DrawTooltip(uint iconId, Vector4 color, string primaryText, string secondaryText = "")
        => DrawStandardTooltipInternal(iconId, 0, color, primaryText, secondaryText);

    public static void DrawTooltip(uint iconId, uint secondIconId, Vector4 color, string primaryText, string secondaryText = "")
        => DrawStandardTooltipInternal(iconId, secondIconId, color, primaryText, secondaryText);

    public static void DrawLevelIcon(Vector2 position, float radius, Viewport viewport, Map map, uint iconId, Vector4 color, float scale, float extraRadius)
    {
        iconId = TryReplaceIconId(iconId);
        
        DrawLevelRing(position, radius, viewport, map, color, extraRadius);
        DrawLevelIcon(position, iconId, map, scale);
    }
    
    public static void DrawLevelTooltip(Vector2 position, float radius, Viewport viewport, Map map, uint iconId, Vector4 color, string primaryText, string secondaryText = "")
        => DrawLevelTooltipInternal(position, radius * viewport.Scale, viewport, map, iconId, 0, color, primaryText, secondaryText);
    
    public static void DrawLevelTooltip(Vector2 position, float radius, Viewport viewport, Map map, uint iconId, uint secondIconId, Vector4 color, string primaryText, string secondaryText = "")
        => DrawLevelTooltipInternal(position, radius, viewport, map, iconId, secondIconId, color, primaryText, secondaryText);
    
    public static void DrawIconRotated(uint iconId, GameObject gameObject, float iconScale)
    {
        if (IconCache.Instance.GetIcon(iconId) is not {} texture) return;
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { } mapWindow) return;
        
        var objectPosition = Position.GetObjectPosition(gameObject.Position, map);
        var center = mapWindow.Viewport.GetImGuiWindowDrawPosition(objectPosition);
        var angle = GetObjectRotation(gameObject);
        var size = new Vector2(texture.Width, texture.Height) * iconScale;
        var vectors = GetRotationVectors(angle, center, size);
    
        ImGui.GetWindowDrawList().AddImageQuad(texture.ImGuiHandle, vectors[0], vectors[1], vectors[2], vectors[3]);
    }
}