using System;
using KamiLib.Classes;
using Lumina.Excel.Sheets;

namespace Mappy.Classes.Caches;

public class TripleTriadCache : Cache<uint, bool> {
	protected override bool LoadValue(uint key) {
		try {
			return Service.DataManager.GetExcelSheet<TripleTriad>().GetRow(key) is { RowId: not 0 };
		}
		catch (Exception) {
			// Probably okay to do this, if we feed a completely invalid valid here, then we don't want to save it to the cache.
			// This doesn't get called every frame, so it's okay to ignore the errors.
			return false;
		}
	}
}