using System;
using KamiLib.Classes;
using Lumina.Excel.Sheets;

namespace Mappy.Classes.Caches;

public class GatheringPointIconCache : Cache<uint, uint> {
    protected override uint LoadValue(uint key) {
        var gatheringPoint = Service.DataManager.GetExcelSheet<GatheringPoint>().GetRow(key);
        var gatheringPointBase = Service.DataManager.GetExcelSheet<GatheringPointBase>().GetRow(gatheringPoint.GatheringPointBase.RowId);

        return gatheringPointBase.GatheringType.RowId switch {
            0 => 60438,
            1 => 60437,
            2 => 60433,
            3 => 60432,
            5 => 60445,
            _ => throw new Exception($"Unknown Gathering Type: {gatheringPointBase.GatheringType.RowId}"),
        };
    }
}