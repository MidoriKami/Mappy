using System.Linq;
using KamiLib.Classes;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.Classes.Caches;

public class TooltipCache : Cache<uint, string> {
    protected override string LoadValue(uint key) 
        => Service.DataManager.GetExcelSheet<MapSymbol>()!.FirstOrDefault(marker => marker.Icon == key)?.PlaceName.Value?.Name ?? "";
}