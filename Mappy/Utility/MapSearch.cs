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
    public static IEnumerable<ISearchResult> Search(string searchTerms, int numResults)
        => AetheryteResults(searchTerms)
            .Concat(PointOfInterestResults(searchTerms))
            .Concat(MapResults(searchTerms))
            .OrderBy(result => result.Label)
            .Take(numResults)
            .ToList();

    private static IEnumerable<ISearchResult> AetheryteResults(string searchTerms)
        => LuminaCache<MapMarker>.Instance
            .Where(marker => marker is { DataType: 3, PlaceNameSubtext.Value.Name.RawString: not "" })
            .Where(marker => marker.PlaceNameSubtext.Value!.Name.RawString.ToLowerInvariant().Contains(searchTerms.ToLowerInvariant()))
            .Select(marker => new AetheryteSearchResult(marker.RowId, marker.SubRowId))
            .GroupBy(result => result.MapId)
            .Select(group => group.First());

    private static IEnumerable<ISearchResult> PointOfInterestResults(string searchTerms)
        => LuminaCache<MapMarker>.Instance
            .Where(marker => marker is { DataType: 0, Icon: 60442, PlaceNameSubtext.Value.Name.RawString: not "" })
            .Where(marker => LuminaCache<Map>.Instance.FirstOrDefault(map => map.MapMarkerRange == marker.RowId) is not null)
            .Where(marker => marker.PlaceNameSubtext.Value!.Name.RawString.ToLowerInvariant().Contains(searchTerms.ToLowerInvariant()))
            .Select(group => new PointOfInterestSearchResult(group.RowId, group.SubRowId))            
            .GroupBy(result => result.MapId)
            .Select(group => group.First());
    
    
    private static IEnumerable<ISearchResult> MapResults(string searchTerms)
        => LuminaCache<Map>.Instance
            .Where(map => map is { MapIndex: 0 or 1, Hierarchy: 1, PlaceName.Value.RowId: not 0, IsEvent: false, PriorityUI: not 0 })
            .Where(map => map.PlaceName.Value!.Name.ToDalamudString().TextValue.ToLowerInvariant().Contains(searchTerms.ToLowerInvariant()))
            .Select(map => new MapSearchResult(map.RowId))
            .GroupBy(result => result.MapId)
            .Select(group => group.First());
}