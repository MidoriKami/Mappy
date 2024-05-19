using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using Mappy.Classes;

namespace Mappy.Modules;

public unsafe class HousingModule : ModuleBase {
    public override void ProcessMarker(MarkerInfo markerInfo) {
        if (HousingManager.Instance()->CurrentTerritory is not null) {
            markerInfo.Radius = 0.0f;
        }
    }
}