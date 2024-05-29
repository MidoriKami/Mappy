using KamiLib.Classes;
using TripleTriad = Lumina.Excel.GeneratedSheets.TripleTriad;

namespace Mappy.Classes.Caches;

public class TripleTriadCache : Cache<uint, bool> {
    protected override bool LoadValue(uint key) 
        => Service.DataManager.GetExcelSheet<TripleTriad>()!.GetRow(key) is not null;
}