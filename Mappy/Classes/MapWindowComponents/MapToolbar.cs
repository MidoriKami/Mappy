using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KamiLib.Extensions;
using KamiLib.Window;
using Lumina.Excel.Sheets;
using Mappy.Windows;

namespace Mappy.Classes.MapWindowComponents;

public unsafe class MapToolbar {
	public void Draw() {
		var toolbarSize = new Vector2(ImGui.GetContentRegionMax().X, 33.0f * ImGuiHelpers.GlobalScale);

        using var childBackgroundStyle = ImRaii.PushColor(ImGuiCol.ChildBg, Vector4.Zero with { W = System.SystemConfig.ToolbarFade });
        using var toolbarChild = ImRaii.Child("toolbar_child", toolbarSize);
        if (!toolbarChild) return;
        
        ImGui.SetCursorPos(new Vector2(5.0f, 5.0f));
        
        if (MappyGuiTweaks.IconButton(FontAwesomeIcon.ArrowUp, "up", "Open Parent Map")) {
            var valueArgs = new AtkValue {
                Type = ValueType.Int, 
                Int = 5,
            };

            var returnValue = new AtkValue();
            AgentMap.Instance()->ReceiveEvent(&returnValue, &valueArgs, 1, 0);
        }
        
        ImGui.SameLine();
        
        if (MappyGuiTweaks.IconButton(FontAwesomeIcon.LayerGroup, "layers", "Show Map Layers")) {
            ImGui.OpenPopup("Mappy_Show_Layers");
        }

        DrawLayersContextMenu();
        
        ImGui.SameLine();
        
        using (var _ = ImRaii.PushColor(ImGuiCol.Button, ImGui.GetStyle().GetColor(ImGuiCol.ButtonActive), System.SystemConfig.FollowPlayer)) {
            if (MappyGuiTweaks.IconButton(FontAwesomeIcon.LocationArrow, "follow", "Toggle Follow Player")) {
                System.SystemConfig.FollowPlayer = !System.SystemConfig.FollowPlayer;
        
                if (System.SystemConfig.FollowPlayer) {
                    System.IntegrationsController.OpenOccupiedMap();
                }
            }
        }
        
        ImGui.SameLine();
        
        if (MappyGuiTweaks.IconButton(FontAwesomeIcon.ArrowsToCircle, "centerPlayer", "Center on Player") && Service.ClientState.LocalPlayer is not null) {
            // Don't center on player if we are already following the player.
            if (!System.SystemConfig.FollowPlayer) {
                System.IntegrationsController.OpenOccupiedMap();
                System.MapRenderer.CenterOnGameObject(Service.ClientState.LocalPlayer);
            }
        }
        
        ImGui.SameLine();
        
        if (MappyGuiTweaks.IconButton(FontAwesomeIcon.MapMarked, "centerMap", "Center on Map")) {
            System.SystemConfig.FollowPlayer = false;
            System.MapRenderer.DrawOffset = Vector2.Zero;
        }
        
        ImGui.SameLine();
        
        if (MappyGuiTweaks.IconButton(FontAwesomeIcon.Search, "search", "Search for Map")) {
            System.WindowManager.AddWindow(new MapSelectionWindow {
                SingleSelectionCallback = selection => {
                    if (selection?.Map != null) {
                        if (AgentMap.Instance()->SelectedMapId != selection.Map.RowId) {
                            System.IntegrationsController.OpenMap(selection.Map.RowId);
                        }

                        if (selection.MarkerLocation is {} location) {
                            System.SystemConfig.FollowPlayer = false;
                            System.MapRenderer.DrawOffset = -location + DrawHelpers.GetMapCenterOffsetVector();
                        }
                    }
                },
            }, WindowFlags.OpenImmediately | WindowFlags.RequireLoggedIn);
        }
        
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - 25.0f * ImGuiHelpers.GlobalScale - ImGui.GetStyle().ItemSpacing.X);
        if (MappyGuiTweaks.IconButton(FontAwesomeIcon.Cog, "settings", "Open Settings")) {
            System.ConfigWindow.UnCollapseOrShow();
            ImGui.SetWindowFocus(System.ConfigWindow.WindowName);
        }
	}
    
    private void DrawLayersContextMenu() {
        using var contextMenu = ImRaii.Popup("Mappy_Show_Layers");
        if (!contextMenu) return;

        var currentMap = Service.DataManager.GetExcelSheet<Map>().GetRow(AgentMap.Instance()->SelectedMapId);
        if (currentMap.RowId is 0) return;
        
        // If this is a region map
        if (currentMap.Hierarchy is 3) {
            foreach (var marker in AgentMap.Instance()->MapMarkers) {
                if (!DrawHelpers.IsRegionIcon(marker.MapMarker.IconId)) continue;

                var label = marker.MapMarker.Subtext.AsDalamudSeString();
                
                if (ImGui.MenuItem(label.ToString())) {
                    System.IntegrationsController.OpenMap(marker.DataKey);
                    System.SystemConfig.FollowPlayer = false;
                    System.MapRenderer.DrawOffset = Vector2.Zero;
                }
            }
        }
        
        // Any other map
        else {
            var layers = Service.DataManager.GetExcelSheet<Map>()
                .Where(eachMap => eachMap.PlaceName.RowId == currentMap.PlaceName.RowId)
                .Where(eachMap => eachMap.MapIndex != 0)
                .OrderBy(eachMap => eachMap.MapIndex)
                .ToList();

            if (layers.Count is 0) {
                ImGui.Text("No layers for this map");
            }
        
            foreach (var layer in layers) {
                if (ImGui.MenuItem(layer.PlaceNameSub.Value.Name.ExtractText(), "", AgentMap.Instance()->SelectedMapId == layer.RowId)) {
                    System.IntegrationsController.OpenMap(layer.RowId);
                    System.SystemConfig.FollowPlayer = false;
                    System.MapRenderer.DrawOffset = Vector2.Zero;
                }
            }
        }
    }
}