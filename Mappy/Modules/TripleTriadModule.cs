using Mappy.Classes;

namespace Mappy.Modules;

public class TripleTriadModule : ModuleBase
{
    public override bool ProcessMarker(MarkerInfo markerInfo)
    {
        if (markerInfo is not { ObjectiveId: { } objectiveId }) return false;
        if (!System.TripleTriadCache.GetValue(objectiveId)) return false;

        markerInfo.SecondaryText = () => System.CardRewardCache.GetValue(objectiveId) ?? string.Empty;
        return true;
    }
}