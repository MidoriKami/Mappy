using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
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
    public MapSearchWidget MapSearch { get; } = new();

    private readonly DefaultIconSfxButton followPlayerButton;
    private readonly DefaultIconSfxButton centerOnPlayerButton;
    private readonly DefaultIconSfxButton configurationButton;
    private readonly DefaultIconSfxButton openLockButton;
    private readonly DefaultIconSfxButton closeLockButton;
    private readonly DefaultIconSfxButton centerMapButton;

    public MapToolbar(Window owner)
    {
        this.owner = owner;

        followPlayerButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                static void FollowPlayerFunction(Task? _)
                {
                    MappySystem.SystemConfig.FollowPlayer = !MappySystem.SystemConfig.FollowPlayer;
                    MappyPlugin.System.SaveConfig();
                }
                
                if (MappySystem.MapTextureController.MoveMapToPlayer() is { } validTask)
                {
                    validTask.ContinueWith(FollowPlayerFunction);
                }
                else // Player is already in the current map, follow immediately
                {
                    FollowPlayerFunction(null);
                }
            },
            Label = FontAwesomeIcon.MapMarkerAlt.ToIconString() + "##FollowPlayerButton",
            TooltipText = Strings.FollowPlayer,
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };

        centerOnPlayerButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                static void CenterViewportFunction(Task? _)
                {
                    if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { Viewport: var viewport }) return;
                    if (Service.ClientState.LocalPlayer is not { Position: var playerPosition }) return;
                    if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;

                    viewport.SetViewportCenter(Position.GetObjectPosition(playerPosition, map));
                }

                if (MappySystem.MapTextureController.MoveMapToPlayer() is { } validTask)
                {
                    validTask.ContinueWith(CenterViewportFunction);
                }
                else // Player is already in the current map, follow immediately
                {
                    CenterViewportFunction(null);
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

        openLockButton = new DefaultIconSfxButton
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
        
        closeLockButton = new DefaultIconSfxButton
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

        centerMapButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { Viewport: var viewport }) return;
                
                MappySystem.SystemConfig.FollowPlayer = false;

                viewport.SetViewportCenter(new Vector2(1024.0f, 1024.0f));
                viewport.SetViewportZoom(0.4f);
            },
            Label = FontAwesomeIcon.ArrowsToDot.ToIconString() + "##CloseLockButton",
            TooltipText = Strings.CenterMap,
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };
    }

    public void Draw()
    {
        if (!owner.IsFocused) MapSearch.ShowMapSelectOverlay = false;

        var hoverShow = MappySystem.SystemConfig.ShowToolbarOnHover && Bound.IsCursorInWindow();
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
                DrawCenterMapWidget();
                ImGui.SameLine();
                MapSearch.DrawWidget();
                ImGui.SameLine();
                DrawConfigurationButton();
                ImGui.SameLine();
                DrawLockUnlockWidget();
                ImGui.SameLine();
                DrawCursorPosition();
            }
            ImGui.EndChild();
            ImGui.PopStyleColor();
            
            MapSearch.Draw();
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

                if (ImGui.Selectable($"{subAreaName}##{layer.Id.RawString}", layer.RowId == map.RowId))
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

    private void DrawRecenterOnPlayerWidget() => centerOnPlayerButton.Draw();

    private void DrawCenterMapWidget() => centerMapButton.Draw();

    private void DrawConfigurationButton() => configurationButton.Draw();

    private void DrawLockUnlockWidget()
    {
        if (MappySystem.SystemConfig.HideWindowFrame)
        {
           openLockButton.Draw();
        }
        else
        {
            closeLockButton.Draw();
        }
    }
    
    private void DrawCursorPosition()
    {
        if (MapSearch.ShowMapSelectOverlay) return;
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not {} mapWindow) return;

        var cursorScreenPosition = ImGui.GetMousePos();

        if (Bound.IsBoundedBy(cursorScreenPosition, mapWindow.Viewport.StartPosition, mapWindow.Viewport.StartPosition + mapWindow.Viewport.Size))
        {
            var cursorPosition = Position.GetTexturePosition(ImGui.GetMousePos() - mapWindow.Viewport.StartPosition, map, mapWindow.Viewport);

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