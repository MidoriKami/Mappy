using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using FieldMarker = Lumina.Excel.GeneratedSheets.FieldMarker;
using Map = Lumina.Excel.GeneratedSheets.Map;

namespace Mappy.System.Modules;

public class WaymarkConfig : IconModuleConfigBase
{
    [Disabled]
    public new bool ShowTooltip = false;
}

public unsafe class Waymark : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.Waymarks;
    public override ModuleConfigBase Configuration { get; protected set; } = new WaymarkConfig();

    private readonly List<FieldMarker> fieldMarkers = LuminaCache<FieldMarker>.Instance.Where(marker => marker.MapIcon is not 0).ToList();

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
        var markerSpan = MarkingController.Instance()->FieldMarkerArraySpan;
        var config = GetConfig<WaymarkConfig>();
        
        foreach (var index in Enumerable.Range(0, 8))
        {
            if (markerSpan[index] is { Active: true } marker)
            {
                var position = Position.GetObjectPosition(marker.Position, map);
                    
                DrawUtilities.DrawIcon(GetIconForMarkerIndex(index), position, config.IconScale);
            }
        }
    }
    
    private uint GetIconForMarkerIndex(int index) => fieldMarkers[index].MapIcon;
}