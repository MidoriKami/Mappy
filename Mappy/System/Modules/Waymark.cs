using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using KamiLib.Caching;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using FieldMarker = Lumina.Excel.GeneratedSheets.FieldMarker;
using Map = Lumina.Excel.GeneratedSheets.Map;

namespace Mappy.System.Modules;

public class WaymarkConfig : IModuleConfig, IIconConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 9;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
}

public unsafe class Waymark : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.Waymarks;
    public override IModuleConfig Configuration { get; protected set; } = new WaymarkConfig();

    private readonly List<FieldMarker> fieldMarkers = LuminaCache<FieldMarker>.Instance.Where(marker => marker.MapIcon is not 0).ToList();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!IsPlayerInCurrentMap(map)) return false;
        
        return base.ShouldDrawMarkers(map);
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var markerSpan = MarkingController.Instance()->FieldMarkerArraySpan;
        var config = GetConfig<WaymarkConfig>();
        
        foreach (var index in Enumerable.Range(0, 8))
        {
            if (markerSpan[index] is { Active: true } marker)
            {
                var position = Position.GetObjectPosition(marker.Position, map);
                    
                if(config.ShowIcon) DrawUtilities.DrawIcon(GetIconForMarkerIndex(index), position, config.IconScale);
            }
        }
    }
    
    private uint GetIconForMarkerIndex(int index) => fieldMarkers[index].MapIcon;
}