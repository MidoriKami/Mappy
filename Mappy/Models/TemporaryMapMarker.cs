using System.Numerics;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models.Enums;
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
        
        if (Radius < 5.0f) DrawUtilities.DrawTooltip(IconID, tooltipColor, TooltipText);
        if (Radius >= 5.0f) DrawUtilities.DrawLevelTooltip(Position, Radius * viewport.Scale, viewport, map, IconID, tooltipColor, TooltipText);
    }
}