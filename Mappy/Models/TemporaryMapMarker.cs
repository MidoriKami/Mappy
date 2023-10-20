using System.Numerics;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models.Enums;
using Mappy.System;

namespace Mappy.Models;

public class TemporaryMapMarker {
    public MarkerType Type { get; init; } = MarkerType.Unknown;
    public uint MapID { get; set; }
    public uint IconID { get; init; }
    public Vector2 Position { get; init; } = Vector2.Zero;
    public float Radius { get; init; }
    public string TooltipText { get; init; } = string.Empty;

    public void ShowContextMenu(Viewport viewport, Map map) {
        // Markers that don't have area rings
        if (Type is MarkerType.Flag)
        {
            if (!ImGui.IsItemClicked(ImGuiMouseButton.Right)) return;
            
            MappySystem.ContextMenuController.Show(PopupMenuType.TempFlag);
        } else { // Markers that do have area rings
            var markerLocation = Utility.Position.GetTexturePosition(Position, map);
            var markerScreePosition = markerLocation * viewport.Scale + viewport.StartPosition - viewport.Offset;
            var cursorLocation = ImGui.GetMousePos();
            
            if (Vector2.Distance(markerScreePosition, cursorLocation) > Radius * viewport.Scale) return;
            if (!ImGui.IsMouseClicked(ImGuiMouseButton.Right)) return;

            MappySystem.ContextMenuController.Show(PopupMenuType.TempArea);
        }
    }
}