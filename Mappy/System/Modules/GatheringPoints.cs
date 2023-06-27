using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using KamiLib.Caching;
using KamiLib.Utilities;
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
    public Vector4 TooltipColor { get; set; } = KnownColor.White.AsVector4();
}

public class GatheringPoints : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.GatheringPoint;
    public override IModuleConfig Configuration { get; protected set; } = new GatheringPointConfig();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!IsPlayerInCurrentMap(map)) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    public override void LoadForMap(MapData mapData)
    {
        // Do Nothing.
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<GatheringPointConfig>();
        
        foreach (var obj in Service.ObjectTable)
        {
            if(obj.ObjectKind != ObjectKind.GatheringPoint) continue;

            if(!IsTargetable(obj)) continue;
            
            var iconId = GetIconIdForGatheringNode(obj);
            
            if(config.ShowIcon) DrawUtilities.DrawIcon(iconId, obj, map, config.IconScale);
            if(config.ShowTooltip) DrawTooltip(obj);
        }
    }
    
    private void DrawTooltip(GameObject gameObject)
    {
        if (!ImGui.IsItemHovered()) return;
        var config = GetConfig<GatheringPointConfig>();

        var gatheringPoint = LuminaCache<GatheringPoint>.Instance.GetRow(gameObject.DataId)!;
        var gatheringPointBase = LuminaCache<GatheringPointBase>.Instance.GetRow(gatheringPoint.GatheringPointBase.Row)!;
        
        var displayString = $"Lv. {gatheringPointBase.GatheringLevel} {gameObject.Name.TextValue}";
        if (displayString != string.Empty) DrawUtilities.DrawTooltip(displayString, config.TooltipColor, GetIconIdForGatheringNode(gameObject));
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
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}