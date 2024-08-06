using System.Linq;
using KamiLib.Classes;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.Classes.Caches;

public class AetheryteAethernetCache : Cache<uint, Aetheryte?> {
    protected override Aetheryte? LoadValue(uint key) {
        if (Service.DataManager.GetExcelSheet<Aetheryte>()!.FirstOrDefault(aetheryte => aetheryte.AethernetName.Row == key) is not { AethernetGroup: var aethernetGroup }) return null;
        if (Service.DataManager.GetExcelSheet<Aetheryte>()!.FirstOrDefault(aetheryte => aetheryte.IsAetheryte && aetheryte.AethernetGroup == aethernetGroup) is not { } targetAetheryte) return null;

        return targetAetheryte;
    }
}