using Mappy.Classes;

namespace Mappy.Modules;

public class TripleTriadModule : ModuleBase {
    public override void ProcessMarker(MarkerInfo markerInfo) {
        if (markerInfo is not { ObjectiveId: { } objectiveId } ) return;
        if (!System.TripleTriadCache.GetValue(objectiveId)) return;

        markerInfo.SecondaryText = () => System.CardRewardCache.GetValue(objectiveId) ?? string.Empty;
    }
}