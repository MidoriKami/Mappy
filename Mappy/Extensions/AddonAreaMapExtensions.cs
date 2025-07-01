using FFXIVClientStructs.FFXIV.Client.UI;

namespace Mappy.Extensions;

public static unsafe class AddonAreaMapExtensions {
	public static void ForceOffscreen(this ref AddonAreaMap addon) {
		if (!addon.IsReady) return;
		if (addon.RootNode is null) return;
		
		addon.RootNode->SetPositionFloat(-9001.0f, -9001.0f);
	}

	public static void RestorePosition(this ref AddonAreaMap addon) {
		if (!addon.IsReady) return;
		if (addon.RootNode is null) return;
		
		addon.RootNode->SetPositionFloat(addon.X, addon.Y);
	}

	public static bool IsOffscreen(this ref AddonAreaMap addon) {
		if (!addon.IsReady) return false;
		if (addon.RootNode is null) return false;
		
		var xAdjusted = addon.RootNode->X < -9000.0f;
		var yAdjusted = addon.RootNode->Y < -9000.0f;
		
		return xAdjusted && yAdjusted;
	}
}