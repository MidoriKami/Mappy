using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.Utility;

public static class MapExtensions
{
    public static string GetName(this Map map)
    {
        if (map.PlaceName.Value?.Name.ToDalamudString().TextValue is { } placeName) return placeName;

        return string.Empty;
    }
    
    public static string GetSubName(this Map map)
    {
        if (map.PlaceNameSub.Value?.Name.ToDalamudString().TextValue is { } placeName) return placeName;

        return string.Empty;
    }
}