using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;
using Mappy.Utility;

namespace Mappy.System.Modules;

public class GatheringPoints : ModuleBase {
    public override ModuleName ModuleName => ModuleName.GatheringPoint;
    public override IModuleConfig Configuration { get; protected set; } = new GatheringPointConfig();

    protected override bool ShouldDrawMarkers(Map map) {
        if (!IsPlayerInCurrentMap(map)) return false;
        if (Service.ClientState.LocalPlayer is not { ClassJob.Id: 16 or 17 or 18 }) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    protected override void DrawMarkers(Viewport viewport, Map map) {
        var config = GetConfig<GatheringPointConfig>();
        
        foreach (var obj in Service.ObjectTable) {
            if(obj.ObjectKind != ObjectKind.GatheringPoint) continue;

            if(!IsTargetable(obj)) continue;
            
            var gatheringPoint = LuminaCache<GatheringPoint>.Instance.GetRow(obj.DataId)!;
            var gatheringPointBase = LuminaCache<GatheringPointBase>.Instance.GetRow(gatheringPoint.GatheringPointBase.Row)!;

            DrawUtilities.DrawMapIcon(new MappyMapIcon {
                IconId = GetIconIdForGatheringNode(obj),
                ObjectPosition = new Vector2(obj.Position.X, obj.Position.Z),
                
                Tooltip = $"Lv. {gatheringPointBase.GatheringLevel} {obj.Name.TextValue}",
            }, config, viewport, map);
        }
    }

    private bool IsTargetable(GameObject gameObject) => gameObject.IsTargetable;

    private uint GetIconIdForGatheringNode(GameObject gameObject) {
        var gatheringPoint = LuminaCache<GatheringPoint>.Instance.GetRow(gameObject.DataId)!;
        var gatheringPointBase = LuminaCache<GatheringPointBase>.Instance.GetRow(gatheringPoint.GatheringPointBase.Row)!;

        return gatheringPointBase.GatheringType.Row switch {
            0 => 60438,
            1 => 60437,
            2 => 60433,
            3 => 60432,
            5 => 60445,
            _ => throw new Exception($"Unknown Gathering Type: {gatheringPointBase.GatheringType.Row}")
        };
    }
}