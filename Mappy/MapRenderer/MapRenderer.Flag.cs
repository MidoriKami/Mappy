using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Classes;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    private unsafe void DrawFlag() {
        if (AgentMap.Instance()->FlagMarkerCount is not 0 && AgentMap.Instance()->FlagMapMarkers[0].TerritoryId == AgentMap.Instance()->SelectedTerritoryId) {
            ref var flagMarker = ref AgentMap.Instance()->FlagMapMarkers[0];
            
            DrawHelpers.DrawMapMarker(new MarkerInfo {
                Position = new Vector2(flagMarker.XFloat, flagMarker.YFloat) * Scale * DrawHelpers.GetMapScaleFactor() + DrawHelpers.GetCombinedOffsetVector() * Scale,
                IconId = flagMarker.MapMarker.IconId,
                Offset = DrawPosition,
                Scale = Scale,
            });
        }
    }
}