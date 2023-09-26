using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using KamiLib;
using Mappy.Interfaces;
using Mappy.System;
using Mappy.System.Localization;
using Mappy.Utility;
using Mappy.Views.General;
using Mappy.Views.Windows;

namespace Mappy.Views.Components;

public class MapToolbar : IMapSearchWidget
{
    private readonly Window owner;
    public bool ShowMapSelectOverlay { get; set; }
    public bool ShowQuestListOverlay { get; set; }
    
    private bool showRegionSearchView;
    private bool showTextSearchView;

    private readonly DefaultIconSfxButton mapLayersButton;
    private readonly DefaultIconSfxButton followPlayerButton;
    private readonly DefaultIconSfxButton centerOnPlayerButton;
    private readonly DefaultIconSfxButton configurationButton;
    private readonly DefaultIconSfxButton openLockButton;
    private readonly DefaultIconSfxButton closeLockButton;
    private readonly DefaultIconSfxButton centerMapButton;
    private readonly DefaultIconSfxButton mapRegionSearchButton;
    private readonly DefaultIconSfxButton mapTextSearchButton;
    private readonly DefaultIconSfxButton questListButton;

    private readonly MapSearchView searchView;
    private readonly MapRegionView regionView;
    private readonly QuestListView questListView;

    public MapToolbar(Window owner)
    {
        this.owner = owner;
        searchView = new MapSearchView(this);
        regionView = new MapRegionView(this);
        questListView = new QuestListView(this);

        mapRegionSearchButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                if (!showRegionSearchView)
                {
                    regionView.Show();
                    ShowMapSelectOverlay = true;
                    showRegionSearchView = true;
                    showTextSearchView = false;
                }
                else
                {
                    ShowMapSelectOverlay = false;
                    showRegionSearchView = false;
                    showTextSearchView = false;
                }
            },
            Label = FontAwesomeIcon.Map.ToIconString() + "##MapRegionSearchButton",
            TooltipText = Strings.SearchByRegion,
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };

        mapTextSearchButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                if (!showTextSearchView)
                {
                    searchView.Show();
                    ShowMapSelectOverlay = true;
                    showRegionSearchView = false;
                    showTextSearchView = true;
                }
                else
                {
                    ShowMapSelectOverlay = false;
                    showRegionSearchView = false;
                    showTextSearchView = false;
                }
            },
            Label = FontAwesomeIcon.Search.ToIconString() + "##MapSearchButton",
            TooltipText = Strings.SearchForMap,
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };

        mapLayersButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                ImGui.OpenPopup("MapLayersPopup");
            },
            Label = FontAwesomeIcon.LayerGroup.ToIconString() + "##MapLayersButton",
            TooltipText = Strings.MapLayers,
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };

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
                if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { Viewport: var viewport }) return;
                if (Service.ClientState.LocalPlayer is not { Position: var playerPosition }) return;
                if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;

                MappySystem.MapTextureController.MoveMapToPlayer();
                viewport.SetViewportCenter(Position.GetTexturePosition(playerPosition, map));
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

        questListButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                ShowQuestListOverlay = !ShowQuestListOverlay;
            },
            Label = FontAwesomeIcon.Question.ToIconString() + "##QuestListButton",
            TooltipText = "Show/Hide Quest List for Current Map",
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };
    }

    public void Draw()
    {
        if (!owner.IsFocused) ShowMapSelectOverlay = false;

        var hoverShow = MappySystem.SystemConfig.ShowToolbarOnHover && Bound.IsCursorInWindow();
        var alwaysShow = MappySystem.SystemConfig.AlwaysShowToolbar;
        var focusedShow = owner.IsFocused;
        
        if (focusedShow || alwaysShow || hoverShow || ShowQuestListOverlay)
        {
            var regionAvailable = ImGui.GetContentRegionAvail();
            
            ImGui.SetCursorPos(Vector2.Zero);
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.0f, 0.0f, 0.0f, 0.80f));        
            if (ImGui.BeginChild("###Toolbar", regionAvailable with { Y = 40.0f * ImGuiHelpers.GlobalScale }, true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                DrawMapLayersWidget();
                ImGui.SameLine();
                DrawFollowPlayerWidget();
                ImGui.SameLine();
                centerOnPlayerButton.Draw();
                ImGui.SameLine();
                centerMapButton.Draw();
                ImGui.SameLine();
                mapRegionSearchButton.Draw();
                ImGui.SameLine();
                mapTextSearchButton.Draw();
                ImGui.SameLine();
                configurationButton.Draw();
                ImGui.SameLine();
                DrawLockUnlockWidget();
                ImGui.SameLine();
                questListButton.Draw();
                ImGui.SameLine();
                DrawCursorPosition();
            }

            ImGui.EndChild();
            ImGui.PopStyleColor();
        }

        DrawMapSearch();
        DrawQuestList();
    }

    private void DrawMapSearch()
    {
        if (!ShowMapSelectOverlay || ShowMapSelectOverlay && ImGui.IsKeyPressed(ImGuiKey.Escape))
        {
            ShowMapSelectOverlay = false;
            showRegionSearchView = false;
            showTextSearchView = false;
            return;
        }

        if (showRegionSearchView) regionView.Draw();
        if (showTextSearchView) searchView.Draw();
    }

    private void DrawQuestList()
    {
        if (!ShowQuestListOverlay) {return;}

        questListView.Draw();
    }
    
    private void DrawMapLayersWidget()
    {
        mapLayersButton.Draw();

        if (ImGui.BeginPopup("MapLayersPopup"))
        {
            if (MappySystem.MapTextureController is { Ready: true, MapLayers: var layers, CurrentMap: var map })
            {
                if (layers.Count is 0)
                {
                    ImGui.TextColored(KnownColor.Gray.Vector(), Strings.NoLayersInfo);
                }
            
                foreach (var layer in layers)
                {
                    var subAreaName = layer.GetSubName();
                    
                    if(subAreaName == string.Empty) continue;

                    if (ImGui.Selectable($"{subAreaName}##{layer.Id.RawString}", layer.RowId == map.RowId))
                    {
                        MappySystem.MapTextureController.LoadMap(layer.RowId);
                    }
                }
            }

            ImGui.EndPopup();
        }
    }

    private void DrawFollowPlayerWidget()
    {
        var followPlayer = MappySystem.SystemConfig.FollowPlayer;

        if (followPlayer) ImGui.PushStyleColor(ImGuiCol.Button, KnownColor.Red.Vector());
        followPlayerButton.Draw();
        if (followPlayer) ImGui.PopStyleColor();
    }

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
        if (ShowMapSelectOverlay) return;
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not {} mapWindow) return;

        var cursorScreenPosition = ImGui.GetMousePos();

        if (Bound.IsBoundedBy(cursorScreenPosition, mapWindow.Viewport.StartPosition, mapWindow.Viewport.StartPosition + mapWindow.Viewport.Size))
        {
            var cursorPosition = Position.GetRawTexturePosition(ImGui.GetMousePos() - mapWindow.Viewport.StartPosition, map, mapWindow.Viewport);

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