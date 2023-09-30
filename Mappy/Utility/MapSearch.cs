using System.Collections.Generic;
using System.Linq;
using Dalamud.Utility;
using KamiLib.Caching;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models.Search;

namespace Mappy.Utility;

public static class MapSearch
{
    public static List<ISearchResult> Search(string searchTerms)
        => AetheryteResults(searchTerms)
            .Concat(PointOfInterestResults(searchTerms))
            .Concat(MapResults(searchTerms))
            .OrderBy(result => result.Label)
            .ToList();

    public static List<ISearchResult> SearchByMapId(uint map)
        => AetheryteResults(string.Empty)
            .Concat(PointOfInterestResults(string.Empty))
            .Concat(MapResults(string.Empty))
            .Where(result => result.MapId == map)
            .OrderBy(result => result.Label)
            .ToList();

    private static IEnumerable<ISearchResult> AetheryteResults(string searchTerms)
        => LuminaCache<MapMarker>.Instance
            .Where(marker => marker is { DataType: 3 })
            .Where(marker => LuminaCache<Aetheryte>.Instance.GetRow(marker.DataKey) is { PlaceName.Value.Name.RawString: var aetheryteName } &&
                             aetheryteName.ToLowerInvariant().Contains(searchTerms.ToLowerInvariant()))
            .Select(marker => new AetheryteSearchResult(marker.RowId, marker.SubRowId));

    private static IEnumerable<ISearchResult> PointOfInterestResults(string searchTerms)
        => LuminaCache<MapMarker>.Instance
            .Where(marker => marker is { DataType: 0, Icon: 60442, PlaceNameSubtext.Value.Name.RawString: not "" })
            .Where(marker => LuminaCache<Map>.Instance.FirstOrDefault(map => map.MapMarkerRange == marker.RowId) is { PriorityCategoryUI: not 0, PriorityUI: not 0 })
            .Where(marker => marker.PlaceNameSubtext.Value!.Name.RawString.ToLowerInvariant().Contains(searchTerms.ToLowerInvariant()))
            .Select(group => new PointOfInterestSearchResult(group.RowId, group.SubRowId));

    private static IEnumerable<ISearchResult> MapResults(string searchTerms)
        => LuminaCache<Map>.Instance
            .Where(map => map is { Hierarchy: 1, PlaceName.Value.RowId: not 0, IsEvent: false })
            .Where(map => map.PlaceName.Value!.Name.ToDalamudString().TextValue.ToLowerInvariant().Contains(searchTerms.ToLowerInvariant()))
            .Select(map => new MapSearchResult(map.RowId));
}