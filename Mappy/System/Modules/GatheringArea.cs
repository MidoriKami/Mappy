﻿using System.Drawing;
using System.Numerics;
using ImGuiNET;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;

namespace Mappy.System.Modules;

public class GatheringAreaConfig : IconModuleConfigBase
{
    [ColorConfigOption("CircleColor", "ModuleColors", 1, 65, 105, 225, 45)]
    public Vector4 CircleColor = KnownColor.RoyalBlue.AsVector4() with { W = 0.33f };
    
    [ColorConfigOption("TooltipColor", "ModuleColors", 1, 1.0f, 1.0f, 1.0f, 1.0f)]
    public Vector4 TooltipColor = KnownColor.White.AsVector4();
}

public class GatheringArea : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.GatheringArea;
    public override ModuleConfigBase Configuration { get; protected set; } = new GatheringAreaConfig();
        
    public static TemporaryMapMarker? TempMapMarker { get; private set; }
    
    protected override bool ShouldDrawMarkers(Map map)
    {
        if (TempMapMarker is null) return false;
        if (TempMapMarker.MapID != map.RowId) return false;
        
        return base.ShouldDrawMarkers(map);
    }
    
    public override void LoadForMap(MapData mapData)
    {
        // Do Nothing
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        if (TempMapMarker is null) return;
        
        var config = GetConfig<GatheringAreaConfig>();

        var markerPosition = Position.GetTextureOffsetPosition(TempMapMarker.Position, map);
        
        DrawRing(viewport, map);
        DrawUtilities.DrawIcon(TempMapMarker.IconID, markerPosition, config.IconScale);
        DrawTooltip();
        ShowContextMenu();
    }
        
    private void ShowContextMenu()
    {
        if (!ImGui.IsItemClicked(ImGuiMouseButton.Right)) return;
        MappySystem.ContextMenuController.Show(ContextMenuType.GatheringArea);
    }
    
    private void DrawRing(Viewport viewport, Map map)
    {
        if (TempMapMarker is null) return;
        
        var config = GetConfig<GatheringAreaConfig>();

        var markerPosition = Position.GetTextureOffsetPosition(TempMapMarker.Position, map);
        var drawPosition = viewport.GetImGuiWindowDrawPosition(markerPosition);

        var radius = TempMapMarker.Radius * viewport.Scale;
        var color = ImGui.GetColorU32(config.CircleColor);
        
        ImGui.GetWindowDrawList().AddCircleFilled(drawPosition, radius, color);
        ImGui.GetWindowDrawList().AddCircle(drawPosition, radius, color, 0, 4);
    }
        
    private void DrawTooltip()
    {
        if (TempMapMarker is null) return;
        if (TempMapMarker.TooltipText == string.Empty) return;
        if (!ImGui.IsItemHovered()) return;
        
        var config = GetConfig<GatheringAreaConfig>();
        
        DrawUtilities.DrawTooltip(TempMapMarker.TooltipText, config.TooltipColor);
    }
    
    public static void SetGatheringAreaMarker(TemporaryMapMarker marker) => TempMapMarker = marker;
    public static void RemoveGatheringAreaMarker() => TempMapMarker = null;
}