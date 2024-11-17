using KamiLib.Classes;
using Lumina.Excel.Sheets;

namespace Mappy.Classes.Caches;

public class TripleTriadCache : Cache<uint, bool> {
	protected override bool LoadValue(uint key)
		=> Service.DataManager.GetExcelSheet<TripleTriad>().HasRow(key);
}