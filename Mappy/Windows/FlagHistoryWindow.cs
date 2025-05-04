using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using KamiLib.Window;
using Mappy.Data;

namespace Mappy.Windows;

public class FlagHistoryWindow : Window {
	public FlagHistoryWindow() : base("Mappy Flag History Window", new Vector2(400.0f, 600.0f)) {
		AdditionalInfoTooltip = "Shows a list of all recently used flags";
	}
	
	public override void PreOpenCheck() {
		if (!System.MapWindow.IsOpen) IsOpen = false;
	}

	protected override void DrawContents() {
		foreach (var flag in System.FlagConfig.FlagHistory) {
			DrawFlag(flag);
			ImGui.Spacing();
		}
	}

	private void DrawFlag(Flag flag) {
		using var id = ImRaii.PushId(flag.GetIdString());
		
		using (ImRaii.Child("flag_container", new Vector2(ImGui.GetContentRegionAvail().X, 85.0f * ImGuiHelpers.GlobalScale))) {
			using (ImRaii.Child("flag_image_container", new Vector2(155.0f * ImGuiHelpers.GlobalScale, ImGui.GetContentRegionAvail().Y))) {
				DrawFlagImage(flag);
			}

			ImGui.SameLine();
			
			using (ImRaii.Child("flag_contents_container", ImGui.GetContentRegionAvail())) {
				DrawFlagData(flag);
				DrawPlaceFlagButton(flag);
			}
		}
	}

	private void DrawFlagImage(Flag flag) {
		var texture = flag.GetMapTexture();
		if (texture is not null) {
			ImGui.Image(texture.ImGuiHandle, ImGui.GetContentRegionAvail(), new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f));
		}
		else {
			ImGuiHelpers.ScaledDummy(ImGui.GetContentRegionAvail());
		}
	}

	private void DrawFlagData(Flag flag) {
		ImGui.Text(flag.GetMap().PlaceName.Value.Name.ExtractText());
		
		ImGui.SameLine();
		var flagCoordinate = flag.GetMapCoordinate();
		var coordinateString = $"{flagCoordinate.X:F1}, {flagCoordinate.Y:F1}";
		var coordinateStringSize = ImGui.CalcTextSize(coordinateString);
		ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - coordinateStringSize.X);
		ImGui.Text(coordinateString);
		
		ImGui.TextColored(KnownColor.Gray.Vector().Lighten(0.20f), flag.GetTerritoryType().PlaceNameZone.Value.Name.ExtractText());
	}

	private void DrawPlaceFlagButton(Flag flag) {
		var buttonSize = ImGuiHelpers.ScaledVector2(100.0f, 24.0f);
		
		ImGui.SetCursorPos(new Vector2(0.0f, ImGui.GetContentRegionMax().Y - buttonSize.Y));
		if (ImGui.Button("Focus", buttonSize)) {
			flag.Focus();
		}
		
		ImGui.SetCursorPos(ImGui.GetContentRegionMax() - buttonSize);
		if (ImGui.Button("Place", buttonSize)) {
			flag.PlaceFlag();
		}
	}
}