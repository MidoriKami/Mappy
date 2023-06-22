using System.Collections.Generic;
using System.Linq;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.Utility;

public record SearchResult(string Label, uint MapID);

public static class MapSearch
{
    public static IEnumerable<SearchResult> Search(string searchTerms, int numResults)
    {
        return Service.DataManager.GetExcelSheet<Map>()!
            .Where(eachMap => eachMap.MapIndex is 0 or 1)
            .Where(eachMap => eachMap.Hierarchy == 1)
            .Where(map => map.PlaceName.Row != 0)
            .Where(map => map.PlaceName.Value is not null)
            .GroupBy(map => map.PlaceName.Value!.Name.ToDalamudString().TextValue)
            .Select(map => map.First())
            .Where(map =>
                map.PlaceName.Value!.Name.ToDalamudString().TextValue.ToLower().Contains(searchTerms.ToLower()))
            .Select(map => new SearchResult(map.PlaceName.Value!.Name.ToDalamudString().TextValue, map.RowId))
            .OrderBy(searchResult => searchResult.Label)
            .Take(numResults);
    }
}