using System.Numerics;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models.Enums;
using Mappy.System;
using Mappy.Utility;

namespace Mappy.Models;

public class TemporaryMapMarker
{
    public MarkerType Type { get; init; } = MarkerType.Unknown;
    public uint MapID { get; set; }
    public uint IconID { get; init; }
    public Vector2 Position { get; init; } = Vector2.Zero;
    public float Radius { get; init; }
    public string TooltipText { get; init; } = string.Empty;

    public void DrawRing(Viewport viewport, Map map, Vector4 circleColor)
    {
        var markerPosition = Utility.Position.GetTextureOffsetPosition(Position, map);
        var drawPosition = viewport.GetImGuiWindowDrawPosition(markerPosition);

        var radius = Radius * viewport.Scale;
        var color = ImGui.GetColorU32(circleColor);
        
        ImGui.GetWindowDrawList().AddCircleFilled(drawPosition, radius, color);
        ImGui.GetWindowDrawList().AddCircle(drawPosition, radius, color, 0, 4);
    }

    public unsafe void DrawIcon(Map map, float scale)
    {
        if (Type is MarkerType.Flag)
        {
            if (AgentMap.Instance() is null) return;
            if (AgentMap.Instance()->IsFlagMarkerSet == 0) return;
        }
        
        DrawUtilities.DrawIcon(IconID, Utility.Position.GetTextureOffsetPosition(Position, map), scale);
    }

    public void DrawTooltip(Viewport viewport, Map map, Vector4 tooltipColor)
    {
        if (TooltipText.IsNullOrEmpty()) return;

        DrawUtilities.DrawLevelTooltip(Position, Radius, viewport, map, IconID, tooltipColor, TooltipText);
    }

    public void ShowContextMenu(Viewport viewport, Map map)
    {
        // Markers that don't have area rings
        if (Type is MarkerType.Flag)
        {
            if (!ImGui.IsItemHovered()) return;
            
            MappySystem.ContextMenuController.Show(ContextMenuType.Flag, viewport, map);
        }
        else // Markers that do have area rings
        {
            var markerLocation = Utility.Position.GetTextureOffsetPosition(Position, map);
            var markerScreePosition = markerLocation * viewport.Scale + viewport.StartPosition - viewport.Offset;
            var cursorLocation = ImGui.GetMousePos();
            
            if (Vector2.Distance(markerScreePosition, cursorLocation) > Radius * viewport.Scale) return;

            var contextType = Type switch
            {
                MarkerType.Gathering => ContextMenuType.GatheringArea,
                MarkerType.Command => ContextMenuType.Command,
                MarkerType.Quest => ContextMenuType.Quest,
                _ => ContextMenuType.Inactive
            };
        
            MappySystem.ContextMenuController.Show(contextType, viewport, map);
        }
    }
}