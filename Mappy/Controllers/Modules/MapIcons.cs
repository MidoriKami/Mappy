// ReSharper disable CollectionNeverUpdated.Global
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.DataModels;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;

namespace Mappy.System.Modules;

public class MapIcons : ModuleBase {
    public override ModuleName ModuleName => ModuleName.MapMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new MapIconConfig();

    private readonly ConcurrentBag<MapMarkerData> mapMarkers = new();

    public override void LoadForMap(MapData mapData) => Task.Run(() => {
        mapMarkers.Clear();
        
        foreach (var row in LuminaCache<MapMarker>.Instance) {
            if (row.RowId == mapData.Map.MapMarkerRange) {
                mapMarkers.Add(new MapMarkerData(row, GetConfig<MapIconConfig>()));
            }
        }
    });

    protected override void DrawMarkers(Viewport viewport, Map map) {
        var config = GetConfig<MapIconConfig>();
        
        foreach (var marker in mapMarkers) {
            if (config.AetherytesOnTop && marker.Type is MapMarkerType.Aetheryte) continue;

            marker.Draw(viewport, map);
        }

        if (config.AetherytesOnTop) {
            foreach (var aetheryte in mapMarkers.Where(marker => marker.Type is MapMarkerType.Aetheryte)) {
                aetheryte.Draw(viewport, map);
            }
        }
    }
}