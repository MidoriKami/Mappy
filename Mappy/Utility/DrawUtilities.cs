using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using KamiLib;
using KamiLib.Caching;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;
using Mappy.System;
using Mappy.Views.Windows;

namespace Mappy.Utility;

public partial class DrawUtilities
{
    public static void DrawMapIcon(MappyMapIcon iconData, Viewport viewport, Map map)
    {
        if (iconData.IconId is 0) return;
        
        iconData.IconId = TryReplaceIconId(iconData.IconId);

        var drawPosition = iconData.GetDrawPosition(map);
        
        if (iconData is { ShowIcon: true } && !IsIconDisabled(iconData.IconId))
        {
            if (iconData is { Radius: > 1.0f })
            {
                DrawAreaRing(drawPosition, iconData.Radius, viewport, map, iconData.RadiusColor);
            }
            
            DrawIconTexture(iconData.IconTexture, viewport, drawPosition, iconData.IconScale);

            if (ImGui.IsItemClicked())
            {
                iconData.OnClickAction?.Invoke();
            }

            var radiusSize = iconData.Radius * viewport.Scale;
            var iconSize = iconData.IconSize.X * iconData.IconScale / 2.0f;
            var isIconBiggerThanRadius = iconSize >= radiusSize;

            switch (iconData)
            {
                case { ShowTooltip: true, Radius: <= 1.0f }:
                case { ShowTooltip: true, Radius: > 1.0f } when isIconBiggerThanRadius:
                    DrawTooltip(iconData.IconId, iconData.TooltipExtraIcon, iconData.TooltipColor, iconData.Tooltip, iconData.TooltipDescription);
                    break;
                
                case { ShowTooltip: true, Radius: > 1.0f } when !isIconBiggerThanRadius:
                    DrawAreaTooltip(drawPosition, iconData.Radius, viewport, iconData.IconId, iconData.TooltipColor, iconData.Tooltip, iconData.TooltipDescription);
                    break;
            }
            
            if (iconData is { ShowDirectionalIndicator: true })
            {
                if (GetDirectionalIconLayer(iconData) is { } layer)
                {
                    iconData.Layers.Add(layer);
                }
            }

            foreach (var layer in iconData.Layers.Where(layer => !IsIconDisabled(layer.IconId)))
            {
                DrawIconTexture(layer.IconTexture, viewport, drawPosition + layer.PositionOffset * iconData.IconScale / viewport.Scale, iconData.IconScale);
            }
        }
    }

    public static void DrawTooltip(Vector4 color, string primaryText)
        => DrawStandardTooltipInternal(0, 0, color, primaryText, string.Empty);

    public static void DrawIconRotated(uint iconId, GameObject gameObject, float iconScale)
    {
        if (IconCache.Instance.GetIcon(iconId) is not {} texture) return;
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { } mapWindow) return;
        
        var objectPosition = Position.GetTexturePosition(gameObject.Position, map);
        var center = mapWindow.Viewport.GetImGuiWindowDrawPosition(objectPosition);
        var angle = GetObjectRotation(gameObject);
        var size = new Vector2(texture.Width, texture.Height) * iconScale;
        var vectors = GetRotationVectors(angle, center, size);
    
        ImGui.GetWindowDrawList().AddImageQuad(texture.ImGuiHandle, vectors[0], vectors[1], vectors[2], vectors[3]);
    }
}