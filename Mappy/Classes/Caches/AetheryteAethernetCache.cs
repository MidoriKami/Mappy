using KamiLib.Classes;
using KamiLib.Extensions;
using Lumina.Excel.Sheets;

namespace Mappy.Classes.Caches;

public class AetheryteAethernetCache : Cache<uint, Aetheryte?> {
    protected override Aetheryte? LoadValue(uint key) {
        if (Service.DataManager.GetExcelSheet<Aetheryte>().FirstOrNull(aetheryte => aetheryte.AethernetName.RowId == key) is not { AethernetGroup: var aethernetGroup }) return null;
        if (Service.DataManager.GetExcelSheet<Aetheryte>().FirstOrNull(aetheryte => aetheryte.IsAetheryte && aetheryte.AethernetGroup == aethernetGroup) is not { } targetAetheryte) return null;

        return targetAetheryte;
    }
}