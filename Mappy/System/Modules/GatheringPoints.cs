using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;

namespace Mappy.System.Modules;

public class GatheringPointConfig : IModuleConfig, IIconConfig, ITooltipConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 5;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.White.Vector();
}

public class GatheringPoints : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.GatheringPoint;
    public override IModuleConfig Configuration { get; protected set; } = new GatheringPointConfig();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!IsPlayerInCurrentMap(map)) return false;
        if (Service.ClientState.LocalPlayer is not { ClassJob.Id: 16 or 17 or 18 }) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<GatheringPointConfig>();
        
        foreach (var obj in Service.ObjectTable)
        {
            if(obj.ObjectKind != ObjectKind.GatheringPoint) continue;

            if(!IsTargetable(obj)) continue;
            
            var gatheringPoint = LuminaCache<GatheringPoint>.Instance.GetRow(obj.DataId)!;
            var gatheringPointBase = LuminaCache<GatheringPointBase>.Instance.GetRow(gatheringPoint.GatheringPointBase.Row)!;

            DrawUtilities.DrawMapIcon(new MappyMapIcon
            {
                IconId = GetIconIdForGatheringNode(obj),
                ObjectPosition = new Vector2(obj.Position.X, obj.Position.Z),
                
                Tooltip = $"Lv. {gatheringPointBase.GatheringLevel} {obj.Name.TextValue}",
            }, config, viewport, map);
        }
    }

    private bool IsTargetable(GameObject gameObject) => gameObject.IsTargetable;

    private uint GetIconIdForGatheringNode(GameObject gameObject)
    {
        var gatheringPoint = LuminaCache<GatheringPoint>.Instance.GetRow(gameObject.DataId)!;
        var gatheringPointBase = LuminaCache<GatheringPointBase>.Instance.GetRow(gatheringPoint.GatheringPointBase.Row)!;

        return gatheringPointBase.GatheringType.Row switch
        {
            0 => 60438,
            1 => 60437,
            2 => 60433,
            3 => 60432,
            5 => 60445,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}