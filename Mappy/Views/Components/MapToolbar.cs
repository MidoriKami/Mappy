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
using Mappy.Utility;
using Mappy.Views.Windows;

namespace Mappy.Views.Components;

public class MapToolbar
{
    private readonly Window owner;
    public MapSelectWidget MapSelect { get; } = new();

    public MapToolbar(Window owner) => this.owner = owner;

    public void Draw()
    {
        if (!owner.IsFocused) MapSelect.ShowMapSelectOverlay = false;
        
        if (owner.IsFocused || MappySystem.SystemConfig.AlwaysShowToolbar)
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
        ImGui.PushID("FollowPlayerButton");
        ImGui.PushFont(UiBuilder.IconFont);

        var followPlayer = MappySystem.SystemConfig.FollowPlayer;

        if (followPlayer) ImGui.PushStyleColor(ImGuiCol.Button, KnownColor.Red.AsVector4());
        if (ImGui.Button(FontAwesomeIcon.MapMarkerAlt.ToIconString(), ImGuiHelpers.ScaledVector2(23.0f)))
        {
            MappySystem.MapTextureController.MoveMapToPlayer();
            MappySystem.SystemConfig.FollowPlayer = !MappySystem.SystemConfig.FollowPlayer;
            MappyPlugin.System.SaveConfig();
        }
        if (followPlayer) ImGui.PopStyleColor();

        ImGui.PopFont();
        
        if (ImGui.IsItemHovered()) DrawUtilities.DrawTooltip("Follow Player", KnownColor.White.AsVector4());

        ImGui.PopID();
    }

    private void DrawRecenterOnPlayerWidget()
    {
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        
        ImGui.PushID("CenterOnPlayer");
        ImGui.PushFont(UiBuilder.IconFont);

        if (ImGui.Button(FontAwesomeIcon.Crosshairs.ToIconString(), ImGuiHelpers.ScaledVector2(23.0f)))
        {
            MappySystem.MapTextureController.MoveMapToPlayer();

            if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is { } window && Service.ClientState.LocalPlayer is {} player)
            {
                window.Viewport.SetViewportCenter(Position.GetObjectPosition(player.Position, map));
            }
        }

        ImGui.PopFont();
        
        if (ImGui.IsItemHovered()) DrawUtilities.DrawTooltip("Center on Player", KnownColor.White.AsVector4());

        ImGui.PopID();
    }

    private void DrawConfigurationButton()
    {
        ImGui.PushID("ConfigurationButton");
        ImGui.PushFont(UiBuilder.IconFont);

        if (ImGui.Button(FontAwesomeIcon.Cog.ToIconString(), ImGuiHelpers.ScaledVector2(25.0f, 23.0f)))
        {
            if (KamiCommon.WindowManager.GetWindowOfType<ConfigurationWindow>() is {} configurationWindow)
            {
                configurationWindow.IsOpen = !configurationWindow.IsOpen;
                configurationWindow.Collapsed = false;
            }
        }

        ImGui.PopFont();
                
        if (ImGui.IsItemHovered()) DrawUtilities.DrawTooltip("Settings", KnownColor.White.AsVector4());

        ImGui.PopID();
    }
    
    private void DrawLockUnlockWidget()
    {
        ImGui.PushID("LockUnlockWidget");

        if (MappySystem.SystemConfig.HideWindowFrame)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.PushID("OpenLock");
            if (ImGui.Button(FontAwesomeIcon.Unlock.ToIconString(), ImGuiHelpers.ScaledVector2(25.0f, 23.0f)))
            {
                MappySystem.SystemConfig.HideWindowFrame = false;
                MappySystem.SystemConfig.LockWindow = false;
                MappyPlugin.System.SaveConfig();
            }
            ImGui.PopFont();
            
            if (ImGui.IsItemHovered()) DrawUtilities.DrawTooltip("Show and Unlock", KnownColor.White.AsVector4());
            ImGui.PopID();
        }
        else
        {
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.PushID("ClosedLock");
            if (ImGui.Button(FontAwesomeIcon.Lock.ToIconString(), ImGuiHelpers.ScaledVector2(25.0f, 23.0f)))
            {
                MappySystem.SystemConfig.HideWindowFrame = true;
                MappySystem.SystemConfig.LockWindow = true;
                MappyPlugin.System.SaveConfig();
            }
            ImGui.PopFont();
            
            if (ImGui.IsItemHovered()) DrawUtilities.DrawTooltip("Hide and Unlock", KnownColor.White.AsVector4());
            ImGui.PopID();
        }
        
        ImGui.PopID();
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