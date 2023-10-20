using System;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;
using Mappy.Utility;
using ModuleBase = Mappy.Abstracts.ModuleBase;

namespace Mappy.System.Modules;

public unsafe class TemporaryMarkers : ModuleBase {
    public override ModuleName ModuleName => ModuleName.TemporaryMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new TemporaryMarkersConfig();
    
    public static TemporaryMapMarker? FlagMarker { get; private set; }
    public static TemporaryMapMarker? GatheringMarker { get; private set; }

    private int counter;

    protected override void DrawMarkers(Viewport viewport, Map map) {
        var config = GetConfig<TemporaryMarkersConfig>();
        counter++;

        if (GatheringMarker is not null && GatheringMarker.MapID == map.RowId) {
            DrawUtilities.DrawMapIcon(new MappyMapIcon {
                IconId = GatheringMarker.IconID,
                ObjectPosition = GatheringMarker.Position,
                
                Tooltip = GatheringMarker.TooltipText,
                
                Radius = GatheringMarker.Type is MarkerType.Quest ? GetAnimatedRadius(GatheringMarker.Radius, counter, 10.0f) : GatheringMarker.Radius,
                RadiusColor = config.CircleColor,
            }, config, viewport, map);
            
            GatheringMarker.ShowContextMenu(viewport, map);
        }
   
        if (FlagMarker is not null && FlagMarker.MapID == map.RowId && AgentMap.Instance() is not null && AgentMap.Instance()->IsFlagMarkerSet is 1) {
            DrawUtilities.DrawMapIcon(new MappyMapIcon {
                IconId = FlagMarker.IconID,
                ObjectPosition = FlagMarker.Position,
                
                Tooltip = FlagMarker.TooltipText,
            }, config, viewport, map);

            FlagMarker.ShowContextMenu(viewport, map);
        }
    }

    private static float GetAnimatedRadius(float originalRadius, int counter, float minRadius)
        => MathF.Max(minRadius, originalRadius * Pulse(counter));
    
    private static float Pulse(float time) 
        => 0.5f * (1 + MathF.Sin(2.0f * MathF.PI * 0.005f * time));

    public static void SetFlagMarker(TemporaryMapMarker marker) 
        => FlagMarker = marker;

    public static void RemoveFlagMarker() {
        AgentMap.Instance()->IsFlagMarkerSet = 0;
        FlagMarker = null;
    }
    
    public static void SetGatheringMarker(TemporaryMapMarker marker) 
        => GatheringMarker = marker;

    public static void RemoveGatheringMarker() 
        => GatheringMarker = null;
}