using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Mappy.Extensions;

public static class MiniMapGatheringMarkerExtensions
{
    public static void Draw(this MiniMapGatheringMarker marker, Vector2 offset, float scale)
    {
        if (marker.ShouldRender is 0) return;

        marker.MapMarker.Scale = 50;
        marker.MapMarker.Draw(offset, scale);
    }
}