using KamiLib.Classes;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.Classes.Caches;

public class GatheringPointNameCache : Cache<(uint dataId, string name), string> {
    protected override string LoadValue((uint dataId, string name) key) {
        var gatheringPoint = Service.DataManager.GetExcelSheet<GatheringPoint>()!.GetRow(key.dataId)!;
        var gatheringPointBase =  Service.DataManager.GetExcelSheet<GatheringPointBase>()!.GetRow(gatheringPoint.GatheringPointBase.Row)!;

        return $"Lv. {gatheringPointBase.GatheringLevel.ToString()} {key.name}";
    }
}