using System.Drawing;
using System.Numerics;
using DailyDuty;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using KamiLib;
using KamiLib.Utilities;
using Mappy.System;
using Mappy.System.Localization;
using Mappy.Utility;
using Mappy.Views.Windows;

namespace Mappy.Views.Components;

public class MapToolbar
{
    private readonly Window owner;
    public MapSelectWidget MapSelect { get; } = new();

    private readonly DefaultIconSfxButton followPlayerButton;
    private readonly DefaultIconSfxButton centerOnPlayerButton;
    private readonly DefaultIconSfxButton configurationButton;
    private readonly DefaultIconSfxButton OpenLockButton;
    private readonly DefaultIconSfxButton CloseLockButton;

    public MapToolbar(Window owner)
    {
        this.owner = owner;

        followPlayerButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                MappySystem.MapTextureController.MoveMapToPlayer();
                MappySystem.SystemConfig.FollowPlayer = !MappySystem.SystemConfig.FollowPlayer;
                MappyPlugin.System.SaveConfig();
            },
            Label = FontAwesomeIcon.MapMarkerAlt.ToIconString() + "##FollowPlayerButton",
            TooltipText = Strings.FollowPlayer,
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };

        centerOnPlayerButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
                
                MappySystem.MapTextureController.MoveMapToPlayer();

                if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is { } window && Service.ClientState.LocalPlayer is {} player)
                {
                    window.Viewport.SetViewportCenter(Position.GetObjectPosition(player.Position, map));
                }
            },
            Label = FontAwesomeIcon.Crosshairs.ToIconString() + "##CenterOnPlayerButton",
            TooltipText = Strings.CenterOnPlayer,
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };

        configurationButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                if (KamiCommon.WindowManager.GetWindowOfType<ConfigurationWindow>() is {} configurationWindow)
                {
                    configurationWindow.IsOpen = !configurationWindow.IsOpen;
                    configurationWindow.Collapsed = false;
                }
            },
            Label = FontAwesomeIcon.Cog.ToIconString() + "##OpenConfigWindowButton",
            TooltipText = Strings.OpenConfigWindow,
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };

        OpenLockButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                MappySystem.SystemConfig.HideWindowFrame = false;
                MappySystem.SystemConfig.LockWindow = false;
                MappyPlugin.System.SaveConfig();
            },
            Label = FontAwesomeIcon.Unlock.ToIconString() + "##OpenLockButton",
            TooltipText = Strings.ShowAndUnlock,
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };
        
        CloseLockButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                MappySystem.SystemConfig.HideWindowFrame = true;
                MappySystem.SystemConfig.LockWindow = true;
                MappyPlugin.System.SaveConfig();
            },
            Label = FontAwesomeIcon.Lock.ToIconString() + "##CloseLockButton",
            TooltipText = Strings.HideAndLock,
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };
    }

    public void Draw()
    {
        if (!owner.IsFocused) MapSelect.ShowMapSelectOverlay = false;

        var hoverShow = MappySystem.SystemConfig.ShowToolbarOnHover && MapWindow.IsCursorInWindow();
        var alwaysShow = MappySystem.SystemConfig.AlwaysShowToolbar;
        var focusedShow = owner.IsFocused;
        
        if (focusedShow || alwaysShow || hoverShow)
        {
            var regionAvailable = ImGui.GetContentRegionAvail();
            
            ImGui.SetCursorPos(Vector2.Zero);
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.0f, 0.0f, 0.0f, 0.80f));        
            if (ImGui.BeginChild("###Toolbar", regionAvailable with { Y = 40.0f * ImGuiHelpers.GlobalScale }, true))
            {
                DrawMapLayersWidget();
                ImGui.SameLine();
                DrawFollowPlayerWidget();
                ImGui.SameLine();
                DrawRecenterOnPlayerWidget();
                ImGui.SameLine();
                MapSelect.DrawWidget();
                ImGui.SameLine();
                DrawConfigurationButton();
                ImGui.SameLine();
                DrawLockUnlockWidget();
                ImGui.SameLine();
                DrawCursorPosition();
            }
            ImGui.EndChild();
            ImGui.PopStyleColor();
            
            MapSelect.Draw();
        }
    }

    private void DrawMapLayersWidget()
    {
        if (MappySystem.MapTextureController is not { Ready: true, MapLayers: var layers, CurrentMap: var map }) return;
        
        ImGui.PushItemWidth(200.0f * ImGuiHelpers.GlobalScale);
        ImGui.BeginDisabled(layers.Count == 0);
        if (ImGui.BeginCombo("###LayerCombo", map.GetName()))
        {
            foreach (var layer in layers)
            {
                var subAreaName = layer.GetSubName();
                    
                if(subAreaName == string.Empty) continue;

                if (ImGui.Selectable(subAreaName, layer.RowId == map.RowId))
                {
                    MappySystem.MapTextureController.LoadMap(layer.RowId);
                }
            }
            ImGui.EndCombo();
        }
        ImGui.EndDisabled();
    }

    private void DrawFollowPlayerWidget()
    {
        var followPlayer = MappySystem.SystemConfig.FollowPlayer;

        if (followPlayer) ImGui.PushStyleColor(ImGuiCol.Button, KnownColor.Red.AsVector4());
        followPlayerButton.Draw();
        if (followPlayer) ImGui.PopStyleColor();
    }

    private void DrawRecenterOnPlayerWidget()
    {
        centerOnPlayerButton.Draw();
    }

    private void DrawConfigurationButton()
    {
        configurationButton.Draw();
    }
    
    private void DrawLockUnlockWidget()
    {
        if (MappySystem.SystemConfig.HideWindowFrame)
        {
           OpenLockButton.Draw();
        }
        else
        {
            CloseLockButton.Draw();
        }
    }
    
    private void DrawCursorPosition()
    {
        if (MapSelect.ShowMapSelectOverlay) return;
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not {} mapWindow) return;

        var cursorScreenPosition = ImGui.GetMousePos();

        if (Bound.IsBoundedBy(cursorScreenPosition, mapWindow.MapContentsStart, mapWindow.MapContentsStart + mapWindow.Viewport.Size))
        {
            var cursorPosition = Position.GetTexturePosition(ImGui.GetMousePos() - mapWindow.MapContentsStart, map, mapWindow.Viewport);

            var mapCoordinates = MapUtil.WorldToMap(cursorPosition, map);

            var regionAvailable = ImGui.GetContentRegionMax();
            var coordinateString = $"( {mapCoordinates.X:F1}, {mapCoordinates.Y:F1} )";
            var stringSize = ImGui.CalcTextSize(coordinateString);

            var currentPosition = ImGui.GetCursorPos();
            ImGui.SetCursorPos(regionAvailable with {X = regionAvailable.X - stringSize.X, Y = currentPosition.Y});
            ImGui.Text(coordinateString);
        }
    }
}