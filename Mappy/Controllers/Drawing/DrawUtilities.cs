﻿using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using ImGuiNET;
using KamiLib;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;
using Mappy.Models.ModuleConfiguration;
using Mappy.System;
using Mappy.Views.Windows;

namespace Mappy.Utility;

public partial class DrawUtilities {
    public static void DrawMapIcon(MappyMapIcon iconData, object configuration, Viewport viewport, Map map) {
        TryUpdateIconId(iconData);
        if (iconData is { IconId: 0 } or { IconTexture: null }) return;

        var drawPosition = iconData.GetDrawPosition(map);
        
        if (configuration is IIconConfig { ShowIcon: true } iconConfig && !IsIconDisabled(iconData.IconId)) {
            if (iconData is { Radius: > 1.0f }) {
                DrawAreaRing(drawPosition, iconData.Radius, viewport, map, iconData.RadiusColor);
            }
            
            DrawIconTexture(iconData.IconTexture, viewport, drawPosition, GetSpecialIconScale(iconData.IconId, iconConfig.IconScale, viewport), iconData.ColorManipulation);

            if (ImGui.IsItemClicked()) {
                iconData.OnClickAction?.Invoke();
            }

            if (configuration is ITooltipConfig { ShowTooltip: true } tooltipConfig) {
                var radiusSize = iconData.Radius * viewport.Scale;
                var iconSize = iconData.IconTexture.Width * iconConfig.IconScale / 2.0f;
                var isIconBiggerThanRadius = iconSize >= radiusSize;
                var color = iconData.GetTooltipColorFunc?.Invoke() ?? tooltipConfig.TooltipColor;
                
                switch (iconData) {
                    case { Radius: <= 1.0f } when ImGui.IsItemHovered():
                    case { Radius: > 1.0f } when isIconBiggerThanRadius && ImGui.IsItemHovered():
                        DrawTooltip(iconData.IconId, iconData.TooltipExtraIcon, color, iconData.GetTooltip(), iconData.GetTooltipExtraText());
                        break;
                
                    case { Radius: > 1.0f } when !isIconBiggerThanRadius:
                        DrawAreaTooltip(drawPosition, iconData.Radius, viewport, iconData.IconId, color, iconData.GetTooltip(), iconData.GetTooltipExtraText());
                        break;
                }
            }

            foreach (var layer in iconData.Layers.Where(layer => !IsIconDisabled(layer.IconId))) {
                DrawIconTexture(layer.IconTexture, viewport, drawPosition + layer.PositionOffset * iconConfig.IconScale / viewport.Scale, iconConfig.IconScale);
            }

            if (configuration is IDirectionalMarkerConfig { EnableDirectionalMarker: true, DistanceThreshold: var threshold }) {
                if (Service.ClientState is { LocalPlayer.Position.Y: var playerHeight }) {
                    var distance = playerHeight - iconData.VerticalPosition;

                    if (Math.Abs(distance) > threshold) {
                        if (distance > 0) DrawIconTexture(BelowPlayer.IconTexture, viewport, drawPosition + BelowPlayer.PositionOffset * iconConfig.IconScale / viewport.Scale, iconConfig.IconScale);
                        if (distance < 0) DrawIconTexture(AbovePlayer.IconTexture, viewport, drawPosition + AbovePlayer.PositionOffset * iconConfig.IconScale / viewport.Scale, iconConfig.IconScale);
                    }
                }
            }
        }
    }
    
    private static void TryUpdateIconId(MappyMapIcon iconData) {
        var replacementId = TryReplaceIconId(iconData.IconId);

        if (iconData.IconId != replacementId) {
            iconData.IconId = replacementId;
        }
    }

    public static void DrawMapText(MappyMapText textData, Viewport viewport, Map map) {
        if (textData.Text == string.Empty) return;

        ImGui.PushFont(textData.UseLargeFont ? KamiCommon.FontManager.Axis18.ImFont : KamiCommon.FontManager.Axis12.ImFont);

        DrawTextOutlined(textData, viewport, map, false);

        if (ImGui.IsItemHovered()) {
            DrawTextOutlined(textData, viewport, map, true);
        }
        
        ImGui.PopFont();
        
        if (textData is { OnClick: { } clickAction } && ImGui.IsItemClicked()) {
            clickAction();
        }
    }
    
    public static void DrawTextOutlined(Vector2 startPosition, string text, KnownColor textColor = KnownColor.White, KnownColor textOutlineColor = KnownColor.Black) {
        const int outlineThickness = 1;
        var textSize = ImGui.CalcTextSize(text);
        var textSizeOffset = new Vector2(-textSize.X / 2.0f, textSize.Y / 2.0f);

        for (var x = -outlineThickness; x <= outlineThickness; ++x) {
            for (var y = -outlineThickness; y <= outlineThickness; ++y) {
                if (x is 0 && y is 0) continue;

                ImGui.SetCursorPos(startPosition + new Vector2(x, y) + textSizeOffset);
                ImGui.TextColored(textOutlineColor.Vector(), text);
            }
        }
        
        ImGui.SetCursorPos(startPosition + textSizeOffset);
        ImGui.TextColored(textColor.Vector(), text);
    }

    public static void DrawTooltip(Vector4 color, string primaryText) {
        if (!ImGui.IsItemHovered()) return;
        DrawStandardTooltipInternal(0, 0, color, primaryText, string.Empty);
    }

    public static void DrawIconRotated(uint iconId, GameObject gameObject, float iconScale) {
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