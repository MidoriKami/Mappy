using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;
using Mappy.Utility;

namespace Mappy.System.Modules;

public class MapIcons : ModuleBase {
    public override ModuleName ModuleName => ModuleName.MapMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new MapIconConfig();

    private readonly ConcurrentBag<MapMarker> mapMarkers = new();

    public override void LoadForMap(MapData mapData) => Task.Run(() => {
        mapMarkers.Clear();
        
        foreach (var row in LuminaCache<MapMarker>.Instance) {
            if (row.RowId == mapData.Map.MapMarkerRange) {
                mapMarkers.Add(row);
            }
        }
    });

    protected override void UpdateMarkers(Viewport viewport, Map map) {
        var config = GetConfig<MapIconConfig>();
        
        foreach (var marker in mapMarkers) {
            if (config.AetherytesOnTop && marker.GetMarkerType() is MapMarkerType.Aetheryte) continue;

            DrawMarker(marker, map);
        }

        if (config.AetherytesOnTop) {
            foreach (var aetheryte in mapMarkers.Where(marker => marker.GetMarkerType() is MapMarkerType.Aetheryte)) {
                DrawMarker(aetheryte, map);
            }
        }
    }

    private void DrawMarker(MapMarker marker, Map map) {
        var config = GetConfig<MapIconConfig>();
        
        if (marker.Type == 1 && config.ShowSubzoneLabels) {
            // subzone labels
            UpdateIcon((marker.RowId, marker.SubRowId), () => new MappyMapIcon {
                MarkerId = (marker.RowId, marker.SubRowId),
                IconId = marker.Icon,
                TexturePosition = marker.GetPosition(),
                ColorManipulation = new Vector4(0.75f, 0.75f, 0.75f, 1.0f),
            });

            UpdateText((marker.RowId, marker.SubRowId), () => new MappyMapText {
                TextId = (marker.RowId, marker.SubRowId),
                Text = GetTooltipString(marker),
                TexturePosition = marker.GetPosition(),
                UseLargeFont = false,
                TextColor = KnownColor.Black.Vector(),
                OutlineColor = KnownColor.White.Vector(),
                HoverColor = KnownColor.Black.Vector(),
                HoverOutlineColor = KnownColor.White.Vector(),
                OnClick = marker.GetClickAction(),
            });
        } else if (marker.Icon is > 063200 and < 63900 || map.Id.RawString.StartsWith("world")) {
            UpdateIcon((marker.RowId, marker.SubRowId), () => new MappyMapIcon {
                MarkerId = (marker.RowId, marker.SubRowId),
                IconId = marker.Icon,
                TexturePosition = marker.GetPosition(),
                ColorManipulation = new Vector4(0.75f, 0.75f, 0.75f, 1.0f),
            });
            
            UpdateText((marker.RowId, marker.SubRowId), () => new MappyMapText {
                TextId = (marker.RowId, marker.SubRowId),
                Text = GetTooltipString(marker),
                TexturePosition = marker.GetPosition(),
                UseLargeFont = true,
                TextColor = KnownColor.Black.Vector(),
                OutlineColor = KnownColor.White.Vector(),
                HoverColor = KnownColor.White.Vector(),
                HoverOutlineColor = KnownColor.RoyalBlue.Vector(),
                OnClick = marker.GetClickAction(),
            });

        } else {
            UpdateIcon((marker.RowId, marker.SubRowId), () => new MappyMapIcon {
                MarkerId = (marker.RowId, marker.SubRowId),
                IconId = marker.Icon,
                TexturePosition = marker.GetPosition(),
                GetTooltipFunc = () => GetTooltipString(marker),
                GetTooltipExtraTextFunc = config.ShowTeleportCostTooltips ? marker.GetSecondaryTooltipString : () => string.Empty,
                GetTooltipColorFunc = () => GetDisplayColor(marker, config),
                OnClickAction = marker.GetClickAction(),
            }, icon => {
                icon.GetTooltipExtraTextFunc = config.ShowTeleportCostTooltips ? marker.GetSecondaryTooltipString : () => string.Empty;
            });
        }
    }
    
    private string GetTooltipString(MapMarker marker) {
        var config = GetConfig<MapIconConfig>();
        
        if (marker.GetDisplayString() == string.Empty && config.ShowMiscTooltips) {
            if(LuminaCache<MapSymbol>.Instance.FirstOrDefault(symbol => symbol.Icon == marker.Icon) is { PlaceName.Value.Name.RawString: var name }) {
                return name;
            }
        }
        
        return marker.GetDisplayString();
    }
    
    private Vector4 GetDisplayColor(MapMarker marker, MapIconConfig settings) => marker.GetMarkerType() switch {
        MapMarkerType.Standard => settings.TooltipColor,
        MapMarkerType.MapLink => settings.MapLinkColor,
        MapMarkerType.InstanceLink => settings.InstanceLinkColor,
        MapMarkerType.Aetheryte => settings.AetheryteColor,
        MapMarkerType.Aethernet => settings.AethernetColor,
        _ => settings.TooltipColor
    };
}
