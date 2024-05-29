using System.Linq;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Extensions;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    private unsafe void DrawStaticMapMarkers() {
        foreach (var index in Enumerable.Range(0, AgentMap.Instance()->MapMarkerCount)) {
            ref var marker = ref AgentMap.Instance()->MapMarkerInfoArraySpan[index];
            if (marker.MapMarker.IconId is 0) continue;
            
            marker.Draw(DrawPosition, Scale);
        }
    }
}