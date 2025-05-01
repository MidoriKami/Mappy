using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using KamiLib.Window;
using Mappy.Data;
using Mappy.Windows;

namespace Mappy.Classes;

public unsafe class MapContextMenu {
	public void Draw(Vector2 mapDrawOffset) {
	    using var contextMenu = ImRaii.ContextPopup("Mappy_Context_Menu");
        
        if (!contextMenu) return;
        
        if (ImGui.MenuItem("Place Flag")) {
            var cursorPosition = ImGui.GetMousePosOnOpeningCurrentPopup(); // Get initial cursor position (screen relative)
            var mapChildOffset = mapDrawOffset; // Get the screen position we started drawing the map at
            var mapDrawPositionOffset = System.MapRenderer.DrawPosition; // Get the map texture top left offset vector
            var textureClickLocation = (cursorPosition - mapChildOffset - mapDrawPositionOffset) / MapRenderer.MapRenderer.Scale; // Math
            var result = textureClickLocation - new Vector2(1024.0f, 1024.0f); // One of our vectors made the map centered, undo it.
            var scaledResult = result / DrawHelpers.GetMapScaleFactor() + DrawHelpers.GetRawMapOffsetVector(); // Apply offset x/y and scalefactor
                
            AgentMap.Instance()->IsFlagMarkerSet = false;
            AgentMap.Instance()->SetFlagMapMarker(AgentMap.Instance()->SelectedTerritoryId, AgentMap.Instance()->SelectedMapId, scaledResult.X, scaledResult.Y);
            AgentChatLog.Instance()->InsertTextCommandParam(1048, false);
        }
        
        if (ImGui.MenuItem("Remove Flag", AgentMap.Instance()->IsFlagMarkerSet)) {
            AgentMap.Instance()->IsFlagMarkerSet = false;
        }

        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGui.MenuItem("Center on Player", Service.ClientState.LocalPlayer is not null) && Service.ClientState.LocalPlayer is not null) {
            System.IntegrationsController.OpenOccupiedMap();
            System.MapRenderer.CenterOnGameObject(Service.ClientState.LocalPlayer);
        }
        
        if (ImGui.MenuItem("Center on Map")) {
            System.SystemConfig.FollowPlayer = false;
            System.MapRenderer.DrawOffset = Vector2.Zero;
        }

        ImGuiHelpers.ScaledDummy(5.0f);
        
        if (ImGui.MenuItem("Lock Zoom", "", ref System.SystemConfig.ZoomLocked)) {
            SystemConfig.Save();
        }
        
        ImGuiHelpers.ScaledDummy(5.0f);
        
        if (ImGui.MenuItem("Open Quest List", System.WindowManager.GetWindow<QuestListWindow>() is null))  {
            System.WindowManager.AddWindow(new QuestListWindow(), WindowFlags.OpenImmediately | WindowFlags.RequireLoggedIn);
        }

        if (ImGui.MenuItem("Open Fate List", System.WindowManager.GetWindow<FateListWindow>() is null)) {
            System.WindowManager.AddWindow(new FateListWindow(), WindowFlags.OpenImmediately | WindowFlags.RequireLoggedIn);
        }
	}
}