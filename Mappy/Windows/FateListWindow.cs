using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using ImGuiNET;
using KamiLib.Window;
using Mappy.Extensions;

namespace Mappy.Windows;

public class FateListWindow : Window {
	private const float ElementHeight = 48.0f;
    
	public FateListWindow() : base("Mappy Fate List Window", new Vector2(300.0f, 400.0f)) {
		AdditionalInfoTooltip = "Shows Fates for the zone you are currently in";
	}

	public override bool DrawConditions()
		=> System.MapWindow.IsOpen;
	
	protected override unsafe void DrawContents() {
		if (Service.FateTable.Length > 0) {
			foreach (var index in Enumerable.Range(0, Service.FateTable.Length)) {
				var fate = FateManager.Instance()->Fates[index].Value;
                
				var cursorStart = ImGui.GetCursorScreenPos();
				if (ImGui.Selectable($"##{fate->FateId}_Selectable", false, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, ElementHeight * ImGuiHelpers.GlobalScale))) {
					System.IntegrationsController.OpenOccupiedMap();
					System.SystemConfig.FollowPlayer = false;
					System.MapRenderer.DrawOffset = -new Vector2(fate->Location.X, fate->Location.Z);
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
						var timeString = $"{SeIconChar.Clock.ToIconString()} {FateContextExtensions.GetTimeRemaining(fate):mm\\:ss}";
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