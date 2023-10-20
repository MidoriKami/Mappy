using System.Linq;
using System.Numerics;
using DailyDuty.System;
using Dalamud.Utility;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.System;
using Mappy.System.Localization;
using Action = System.Action;

namespace Mappy.Utility; 

public enum MapMarkerType {
    Standard,
    MapLink,
    InstanceLink,
    Aetheryte,
    Aethernet
}

public static class MapMarkerExtensions {
    public static string GetSecondaryTooltipString(this MapMarker marker) => marker.GetMarkerType() switch {
        MapMarkerType.Standard => string.Empty,
        MapMarkerType.MapLink => string.Empty,
        MapMarkerType.InstanceLink => string.Empty,
        MapMarkerType.Aetheryte => $"{Strings.TeleportCost}: {Service.AetheryteList.FirstOrDefault(entry => entry.AetheryteId == marker.DataKey)?.GilCost ?? 0:n0} {Strings.gil}",
        MapMarkerType.Aethernet => string.Empty,
        _ => string.Empty
    };
    
    public static string GetDisplayString(this MapMarker marker) => marker.GetMarkerType() switch {
        MapMarkerType.Standard => marker.GetStandardMarkerString(),
        MapMarkerType.MapLink => marker.GetPlaceNameData().Name.ToDalamudString().TextValue,
        MapMarkerType.InstanceLink => string.Empty,
        MapMarkerType.Aetheryte => marker.GetAetheryteData().PlaceName.Value?.Name.ToDalamudString().TextValue ?? string.Empty,
        MapMarkerType.Aethernet => marker.GetPlaceNameData().Name.ToDalamudString().TextValue,
        _ => string.Empty,
    };
    
    public static Action? GetClickAction(this MapMarker marker) => marker.GetMarkerType() switch {
        MapMarkerType.Standard => null,
        MapMarkerType.MapLink => () => MappySystem.MapTextureController.LoadMap(marker.GetMapData().RowId),
        MapMarkerType.InstanceLink => null,
        MapMarkerType.Aetheryte => () => TeleporterController.Instance.Teleport(marker.GetAetheryteData()),
        MapMarkerType.Aethernet => () => {
            if (LuminaCache<Aetheryte>.Instance.FirstOrDefault(aetheryte => aetheryte.AethernetName.Row == marker.GetPlaceNameData().RowId) is not { AethernetGroup: var aethernetGroup }) return;
            if (LuminaCache<Aetheryte>.Instance.FirstOrDefault(aetheryte => aetheryte.IsAetheryte && aetheryte.AethernetGroup == aethernetGroup) is not { } targetAetheryte) return;

            TeleporterController.Instance.Teleport(targetAetheryte);
        },
        _ => null
    };
    
    private static Map GetMapData(this MapMarker marker)
        => LuminaCache<Map>.Instance.GetRow(marker.DataKey)!;
    
    private static Aetheryte GetAetheryteData(this MapMarker marker)
        => LuminaCache<Aetheryte>.Instance.GetRow(marker.DataKey)!;
    
    private static PlaceName GetPlaceNameData(this MapMarker marker)
        => LuminaCache<PlaceName>.Instance.GetRow(marker.DataKey)!;
    
    public static string GetMarkerLabel(this MapMarker marker)
        => LuminaCache<PlaceName>.Instance.GetRow(marker.PlaceNameSubtext.Row)!.Name.ToDalamudString().TextValue;

    public static Vector2 GetPosition(this MapMarker marker) 
        => new(marker.X, marker.Y);
    
    public static MapMarkerType GetMarkerType(this MapMarker marker)
        => (MapMarkerType) marker.DataType;
    
    private static string GetStandardMarkerString(this MapMarker marker) {
        var placeName = marker.GetMarkerLabel();
        if (placeName != string.Empty) return placeName;

        var mapSymbol = LuminaCache<MapSymbol>.Instance.GetRow(marker.Icon);
        return mapSymbol?.PlaceName.Value?.Name.ToDalamudString().TextValue ?? string.Empty;
    }
}