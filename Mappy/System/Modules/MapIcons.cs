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
using MapMarkerData = Mappy.DataModels.MapMarkerData;

namespace Mappy.System.Modules;

[Category("ModuleColors")]
public interface IMapIconColorConfig
{
    [ColorConfig("MapLinkColor", 0.655f, 0.396f, 0.149f, 1.0f)]
    public Vector4 MapLinkColor { get; set; } 
    
    [ColorConfig("InstanceLinkColor", 255, 165, 0, 255)]
    public Vector4 InstanceLinkColor { get; set; } 
    
    [ColorConfig("AetheryteColor", 65, 105, 225, 255)]
    public Vector4 AetheryteColor { get; set; }
    
    [ColorConfig("AethernetColor", 173, 216, 230, 255)]
    public Vector4 AethernetColor { get; set; } 
}

[Category("ModuleConfig")]
public class MapIconConfig : IModuleConfig, IIconConfig, ITooltipConfig, IMapIconColorConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 1;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    
    public Vector4 TooltipColor { get; set; } = KnownColor.White.AsVector4();
    public Vector4 MapLinkColor { get; set; } = new(0.655f, 0.396f, 0.149f, 1.0f);
    public Vector4 InstanceLinkColor { get; set; } = KnownColor.Orange.AsVector4();
    public Vector4 AetheryteColor { get; set; } = KnownColor.RoyalBlue.AsVector4();
    public Vector4 AethernetColor { get; set; }  = KnownColor.LightBlue.AsVector4();
    
    [BoolConfig("AetherytesOnTop")]
    public bool AetherytesOnTop { get; set; } = true;
    
    [BoolConfig("ShowMiscTooltips", "ShowMiscTooltipsHelp")]
    public bool ShowMiscTooltips { get; set; } = true;
    
    [MapMarkerSelection]
    public HashSet<uint> DisabledMarkers { get; set; } = new();
}

public class MapIcons : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.MapMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new MapIconConfig();

    private readonly ConcurrentBag<MapMarkerData> mapMarkers = new();

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
            
            if(config.ShowIcon) marker.Draw();
        }

        if (config.AetherytesOnTop)
        {
            foreach (var aetheryte in mapMarkers.Where(marker => marker.Type is MapMarkerType.Aetheryte))
            {
                if (config.DisabledMarkers.Contains(aetheryte.IconId)) continue;
                
                if (config.ShowIcon) aetheryte.Draw();
            }
        }
    }
}