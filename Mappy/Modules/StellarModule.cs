using Mappy.Classes;

namespace Mappy.Modules;

public class StellarModule : ModuleBase {
	public override bool ProcessMarker(MarkerInfo markerInfo) {
		if (markerInfo.MarkerType is not MarkerType.Stellar) return false;

		return true;
	}
}