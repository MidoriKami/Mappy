using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Mappy.Extensions;

public static class MapMarkerInfoExtensions {
    public static void Draw(this MapMarkerInfo marker, Vector2 offset, float scale)
        => marker.MapMarker.Draw(offset, scale);
}