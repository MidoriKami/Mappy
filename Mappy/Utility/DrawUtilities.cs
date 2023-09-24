using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using KamiLib;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;
using Mappy.System;
using Mappy.System.Modules;
using Mappy.Views.Windows;

namespace Mappy.Utility;

public partial class DrawUtilities
{
    public static void DrawMapIcon(MappyMapIcon iconData, object configuration, Viewport viewport, Map map)
    {
        if (iconData is { IconId: 0 } or { IconTexture: null }) return;
        
        iconData.IconId = TryReplaceIconId(iconData.IconId);

        var drawPosition = iconData.GetDrawPosition(map);
        
        if (configuration is IIconConfig { ShowIcon: true } iconConfig && !IsIconDisabled(iconData.IconId))
        {
            if (iconData is { Radius: > 1.0f })
            {
                DrawAreaRing(drawPosition, iconData.Radius, viewport, map, iconData.RadiusColor);
            }
            
            DrawIconTexture(iconData.IconTexture, viewport, drawPosition, GetSpecialIconScale(iconData.IconId, iconConfig.IconScale, viewport), iconData.ColorManipulation);

            if (ImGui.IsItemClicked())
            {
                iconData.OnClickAction?.Invoke();
            }

            if (configuration is ITooltipConfig { ShowTooltip: true } tooltipConfig)
            {
                var radiusSize = iconData.Radius * viewport.Scale;
                var iconSize = iconData.IconTexture.Width * iconConfig.IconScale / 2.0f;
                var isIconBiggerThanRadius = iconSize >= radiusSize;
                var color = iconData.GetTooltipColorFunc?.Invoke() ?? tooltipConfig.TooltipColor;
                
                switch (iconData)
                {
                    case { Radius: <= 1.0f } when ImGui.IsItemHovered():
                    case { Radius: > 1.0f } when isIconBiggerThanRadius && ImGui.IsItemHovered():
                        DrawTooltip(iconData.IconId, iconData.TooltipExtraIcon, color, iconData.GetTooltip(), iconData.GetTooltipExtraText());
                        break;
                
                    case { Radius: > 1.0f } when !isIconBiggerThanRadius:
                        DrawAreaTooltip(drawPosition, iconData.Radius, viewport, iconData.IconId, color, iconData.GetTooltip(), iconData.GetTooltipExtraText());
                        break;
                }
            }
            
            if (configuration is IDirectionalMarkerConfig { EnableDirectionalMarker: true } directionalMarkerConfig)
            {
                if (GetDirectionalIconLayer(iconData, directionalMarkerConfig) is { } layer)
                {
                    iconData.Layers.Add(layer);
                }
            }

            foreach (var layer in iconData.Layers.Where(layer => !IsIconDisabled(layer.IconId)))
            {
                DrawIconTexture(layer.IconTexture, viewport, drawPosition + layer.PositionOffset * iconConfig.IconScale / viewport.Scale, iconConfig.IconScale);
            }
        }
    }

    public static void DrawMapText(MappyMapText textData, Viewport viewport, Map map)
    {
        if (textData.Text == string.Empty) return;

        ImGui.PushFont(textData.UseLargeFont ? KamiCommon.FontManager.Axis18.ImFont : KamiCommon.FontManager.Axis12.ImFont);

        DrawTextOutlined(textData, viewport, map, false);

        if (ImGui.IsItemHovered())
        {
            DrawTextOutlined(textData, viewport, map, true);
        }
        
        ImGui.PopFont();
        
        if (textData is { OnClick: { } clickAction } && ImGui.IsItemClicked())
        {
            clickAction();
        }
    }

    public static void DrawTooltip(Vector4 color, string primaryText)
    {
        if (!ImGui.IsItemHovered()) return;
        DrawStandardTooltipInternal(0, 0, color, primaryText, string.Empty);
    }

    public static void DrawIconRotated(uint iconId, GameObject gameObject, float iconScale)
    {
        if (Service.TextureProvider.GetIcon(iconId) is not {} texture) return;
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { } mapWindow) return;
        
        var objectPosition = Position.GetTexturePosition(gameObject.Position, map);
        var center = mapWindow.Viewport.GetImGuiWindowDrawPosition(objectPosition);
        var angle = GetObjectRotation(gameObject);
        var size = texture.Size * iconScale;
        var vectors = GetRotationVectors(angle, center, size);
    
        ImGui.GetWindowDrawList().AddImageQuad(texture.ImGuiHandle, vectors[0], vectors[1], vectors[2], vectors[3]);
    }
}