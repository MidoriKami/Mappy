using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Mappy.Classes.MapWindowComponents;

public unsafe class MapCoordinateBar {
	public void Draw(bool isMapHovered, Vector2 mapDrawOffset) {
        var coordinateBarSize = new Vector2(ImGui.GetContentRegionMax().X, 20.0f * ImGuiHelpers.GlobalScale);
        ImGui.SetCursorPos(ImGui.GetContentRegionMax() - coordinateBarSize);
        
        using var childBackgroundStyle = ImRaii.PushColor(ImGuiCol.ChildBg, Vector4.Zero with { W = System.SystemConfig.CoordinateBarFade });
        using var coordinateChild = ImRaii.Child("coordinate_child", coordinateBarSize);
        if (!coordinateChild) return;

        var offsetX = -AgentMap.Instance()->SelectedOffsetX;
        var offsetY = -AgentMap.Instance()->SelectedOffsetY;
        var scale = AgentMap.Instance()->SelectedMapSizeFactor;

        var characterMapPosition = MapUtil.WorldToMap(Service.ClientState.LocalPlayer?.Position ?? Vector3.Zero, offsetX, offsetY, 0, (uint)scale);
        var characterPosition = $"Character  {characterMapPosition.X:F1}  {characterMapPosition.Y:F1}";
        
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2.0f * ImGuiHelpers.GlobalScale);

        var characterStringSize = ImGui.CalcTextSize(characterPosition);
        ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X / 3.0f - characterStringSize.X / 2.0f);

        if (AgentMap.Instance()->SelectedMapId == AgentMap.Instance()->CurrentMapId) {
            ImGui.TextColored(System.SystemConfig.CoordinateTextColor, characterPosition);
        }

        if (isMapHovered) {
            var cursorPosition = ImGui.GetMousePos() - mapDrawOffset;
            cursorPosition -= System.MapRenderer.DrawPosition;
            cursorPosition /= MapRenderer.MapRenderer.Scale;
            cursorPosition -= new Vector2(1024.0f, 1024.0f);
            cursorPosition -= new Vector2(offsetX, offsetY);
            cursorPosition /= AgentMap.Instance()->SelectedMapSizeFactorFloat;
 
            var cursorMapPosition = MapUtil.WorldToMap(new Vector3(cursorPosition.X, 0.0f, cursorPosition.Y), offsetX, offsetY, 0, (uint)scale);
            var cursorPositionString = $"Cursor  {cursorMapPosition.X:F1}  {cursorMapPosition.Y:F1}";
            var cursorStringSize = ImGui.CalcTextSize(characterPosition);
            ImGui.SameLine(ImGui.GetContentRegionMax().X * 2.0f / 3.0f - cursorStringSize.X / 2.0f);
            ImGui.TextColored(System.SystemConfig.CoordinateTextColor, cursorPositionString);
        }
	}
}