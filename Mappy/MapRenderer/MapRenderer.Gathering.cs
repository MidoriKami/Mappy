using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Extensions;

namespace Mappy.MapRenderer;

public partial class MapRenderer
{
    private unsafe void DrawGatheringMarkers()
    {
        foreach (var marker in AgentMap.Instance()->MiniMapGatheringMarkers) {
            marker.Draw(DrawPosition, Scale);
        }
    }
}