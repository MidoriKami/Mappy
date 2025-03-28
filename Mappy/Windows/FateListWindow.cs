using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using KamiLib.Classes;
using KamiLib.Window;
using Mappy.Data;
using Mappy.Extensions;

namespace Mappy.Windows;

public class FateListWindow : Window {
	private const float ElementHeight = 48.0f;
    
	public FateListWindow() : base("Mappy Fate List Window", new Vector2(300.0f, 400.0f)) {
		AdditionalInfoTooltip = "Shows Fates for the zone you are currently in";
	}

	public override void PreOpenCheck() {
		if (!System.MapWindow.IsOpen) IsOpen = false;
	}
	
	protected override unsafe void DrawContents() {
		if (Service.FateTable.Length > 0) {
			using (var toolbarChild = ImRaii.Child("fatelist_toolbar", new Vector2(ImGui.GetContentRegionAvail().X, 32.0f))) {
				if (toolbarChild) {
					using var color = ImRaii.PushColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int) ImGuiCol.ButtonActive], System.SystemConfig.SetFlagOnFateClick);
					if (ImGuiTweaks.IconButtonWithSize(Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle, FontAwesomeIcon.Flag, "flag_on_click", ImGuiHelpers.ScaledVector2(23.0f), "Set Flag On Click")) {
						System.SystemConfig.SetFlagOnFateClick = !System.SystemConfig.SetFlagOnFateClick;
						SystemConfig.Save();
					}
				}
			}
			
			foreach (var index in Enumerable.Range(0, Service.FateTable.Length)) {
				var fate = FateManager.Instance()->Fates[index].Value;
                
				var cursorStart = ImGui.GetCursorScreenPos();
				if (ImGui.Selectable($"##{fate->FateId}_Selectable", false, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, ElementHeight * ImGuiHelpers.GlobalScale))) {
					System.IntegrationsController.OpenOccupiedMap();
					System.SystemConfig.FollowPlayer = false;
					System.MapRenderer.DrawOffset = -new Vector2(fate->Location.X, fate->Location.Z);

					if (System.SystemConfig.SetFlagOnFateClick) {
						AgentMap.Instance()->IsFlagMarkerSet = false;
						AgentMap.Instance()->SetFlagMapMarker(AgentMap.Instance()->CurrentTerritoryId, AgentMap.Instance()->CurrentMapId, fate->Location.X, fate->Location.Z);
						AgentChatLog.Instance()->InsertTextCommandParam(1048, false);
					}
				}

				ImGui.SetCursorScreenPos(cursorStart);
				using (ImRaii.Child($"image_child_{fate->FateId}", new Vector2(ElementHeight, ElementHeight), false, ImGuiWindowFlags.NoInputs)) {
					ImGui.Image(Service.TextureProvider.GetFromGameIcon(fate->IconId).GetWrapOrEmpty().ImGuiHandle, ImGuiHelpers.ScaledVector2(ElementHeight, ElementHeight));
				}
                    
				ImGui.SameLine();
                    
				using (ImRaii.Child($"text_child_{fate->FateId}", new Vector2(ImGui.GetContentRegionAvail().X, ElementHeight), false, ImGuiWindowFlags.NoInputs)) {
					ImGui.TextColored(FateContextExtensions.GetColor(fate, 1.0f), $"Lv. {fate->Level} {fate->Name}");
					ImGui.TextUnformatted($"Progress: {fate->Progress}%");

					var timeRemaining = FateContextExtensions.GetTimeRemaining(fate);
					if (timeRemaining != TimeSpan.Zero) {
						var timeString = $"{(fate->IsBonus ? "Exp Bonus!\t" : string.Empty)}{SeIconChar.Clock.ToIconString()} {FateContextExtensions.GetTimeRemaining(fate):mm\\:ss}";
						ImGui.SameLine(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(timeString).X);
						ImGui.Text(timeString);
					}
				}
			}
		}
		else {
			const string text = "No FATE's available";
			var textSize = ImGui.CalcTextSize(text);
			ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2.0f - textSize.X / 2.0f);
			ImGui.SetCursorPosY(ImGui.GetContentRegionAvail().Y / 2.0f - textSize.Y / 2.0f);
			ImGui.TextColored(KnownColor.Orange.Vector(), text);
		}
	}
    
	public override void OnClose() {
		System.WindowManager.RemoveWindow(this);
	}
}