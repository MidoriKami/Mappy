using KamiLib.Classes;
using Lumina.Excel.Sheets;

namespace Mappy.Classes.Caches;

public class GatheringPointNameCache : Cache<(uint dataId, string name), string>
{
    protected override string LoadValue((uint dataId, string name) key)
    {
        var gatheringPoint = Service.DataManager.GetExcelSheet<GatheringPoint>().GetRow(key.dataId);
        var gatheringPointBase = Service.DataManager.GetExcelSheet<GatheringPointBase>().GetRow(gatheringPoint.GatheringPointBase.RowId);

        return $"Lv. {gatheringPointBase.GatheringLevel.ToString()} {key.name}";
    }
}