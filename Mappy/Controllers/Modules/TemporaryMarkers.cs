using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;
using ModuleBase = Mappy.Abstracts.ModuleBase;

namespace Mappy.System.Modules;

public unsafe class TemporaryMarkers : ModuleBase {
    public override ModuleName ModuleName => ModuleName.TemporaryMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new TemporaryMarkersConfig();
    
    public static TemporaryMapMarker? FlagMarker { get; private set; }
    public static TemporaryMapMarker? GatheringMarker { get; private set; }

    protected override void UpdateMarkers(Viewport viewport, Map map) {
        var config = GetConfig<TemporaryMarkersConfig>();

        if (GatheringMarker is not null && GatheringMarker.MapID == map.RowId) {
            UpdateIcon(GatheringMarker, () => new MappyMapIcon {
                MarkerId = GatheringMarker,
                IconId = GatheringMarker.IconID,
                ObjectPosition = GatheringMarker.Position,
                Tooltip = GatheringMarker.TooltipText,
                Animate = GatheringMarker.Type is MarkerType.Quest,
                MinimumRadius = GatheringMarker.Radius,
                RadiusColor = config.CircleColor,
            });
            
            GatheringMarker.ShowContextMenu(viewport, map);
        }
   
        if (FlagMarker is not null && FlagMarker.MapID == map.RowId && AgentMap.Instance() is not null && AgentMap.Instance()->IsFlagMarkerSet is 1) {
            UpdateIcon(FlagMarker, () => new MappyMapIcon {
                MarkerId = FlagMarker,
                IconId = FlagMarker.IconID,
                ObjectPosition = FlagMarker.Position,
                Tooltip = FlagMarker.TooltipText
            });

            FlagMarker.ShowContextMenu(viewport, map);
        }
    }

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