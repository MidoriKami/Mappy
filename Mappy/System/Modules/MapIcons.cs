// ReSharper disable CollectionNeverUpdated.Global
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.DataModels;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Views.Attributes;

namespace Mappy.System.Modules;

public class MapIconConfig : IconModuleConfigBase
{
    [BoolConfigOption("AetherytesOnTop", "ModuleConfig", 0)]
    public bool AetherytesOnTop = true;
    
    [ColorConfigOption("StandardColor", "ModuleColors", 1, 255, 255, 255, 255)]
    public Vector4 StandardColor = KnownColor.White.AsVector4();
    
    [ColorConfigOption("MapLinkColor", "ModuleColors", 1, 0.655f, 0.396f, 0.149f, 1.0f)]
    public Vector4 MapLinkColor = new(x: 0.655f, 0.396f, 0.149f, 1.0f);
    
    [ColorConfigOption("InstanceLinkColor", "ModuleColors", 1, 255, 165, 0, 255)]
    public Vector4 InstanceLinkColor = KnownColor.Orange.AsVector4();
    
    [ColorConfigOption("AetheryteColor", "ModuleColors", 1, 65, 105, 225, 255)]
    public Vector4 AetheryteColor = KnownColor.RoyalBlue.AsVector4();
    
    [ColorConfigOption("AethernetColor", "ModuleColors", 1, 173, 216, 230, 255)]
    public Vector4 AethernetColor = KnownColor.LightBlue.AsVector4();
    
    [MapMarkerSelection(null, "DisplayedMarkers", 3)]
    public HashSet<uint> DisabledMarkers = new();
}

public class MapIcons : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.MapMarkers;
    public override ModuleConfigBase Configuration { get; protected set; } = new MapIconConfig();

    private readonly ConcurrentBag<MapMarkerData> mapMarkers = new();

    protected override bool ShouldDrawMarkers(Map map)
    {
        var config = GetConfig<MapIconConfig>();
        
        if (!config.ShowIcon) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    public override void LoadForMap(MapData mapData) => Task.Run(() =>
    {
        mapMarkers.Clear();
        
        foreach (var row in LuminaCache<MapMarker>.Instance)
        {
            if (row.RowId == mapData.Map.MapMarkerRange)
            {
                mapMarkers.Add(new MapMarkerData(row, GetConfig<MapIconConfig>()));
            }
        }
    });

    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<MapIconConfig>();
        
        foreach (var marker in mapMarkers)
        {
            if (config.DisabledMarkers.Contains(marker.IconId)) continue;
            if (config.AetherytesOnTop && marker.Type is MapMarkerType.Aetheryte) continue; 
            
            marker.Draw();
        }

        if (config.AetherytesOnTop)
        {
            foreach (var aetheryte in mapMarkers.Where(marker => marker.Type is MapMarkerType.Aetheryte))
            {
                if (config.DisabledMarkers.Contains(aetheryte.IconId)) continue;

                aetheryte.Draw();
            }
        }
    }
}