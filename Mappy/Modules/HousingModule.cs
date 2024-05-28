using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using Mappy.Classes;

namespace Mappy.Modules;

public unsafe class HousingModule : ModuleBase {
    public override bool ProcessMarker(MarkerInfo markerInfo) {
        if (HousingManager.Instance()->CurrentTerritory is not null) {
            markerInfo.Radius = 0.0f;
        }
        
        // Todo: somehow draw text for housing number
        return false;
    }
}