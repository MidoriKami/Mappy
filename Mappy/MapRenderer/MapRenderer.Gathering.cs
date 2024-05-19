using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Extensions;

namespace Mappy.Classes;

public partial class MapRenderer {
    private unsafe void DrawGatheringMarkers() {
        foreach (var marker in AgentMap.Instance()->MiniMapGatheringMarkersSpan) {
            marker.Draw(DrawPosition, Scale);
        }
    }
}