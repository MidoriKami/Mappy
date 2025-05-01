using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.Extensions;
using Mappy.Extensions;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    private unsafe void DrawTemporaryMarkers() {
        if (AgentMap.Instance()->SelectedMapSub != AgentMap.Instance()->SelectedMapId) return;
        
        // Group together icons based on their dataId, this is because square enix shows circles then draws the actual icon overtop
        var validMarkers = new Span<TempMapMarker>(Unsafe.AsPointer(ref AgentMap.Instance()->TempMapMarkers[0]), AgentMap.Instance()->TempMapMarkers.Length);
        var iconGroups = validMarkers.ToArray().GroupBy(markers => new Vector2(markers.MapMarker.X, markers.MapMarker.Y));
        
        foreach (var group in iconGroups) {
            // Make a copy of the first marker in the set, we will be mutating this copy.
            var markerCopy = group.First();
        
            // Get the actual iconId we want, typically the icon for the marker, not the circle
            var correctIconId = group.FirstOrNull(marker => marker.MapMarker.IconId is not 60493);
            markerCopy.MapMarker.IconId = correctIconId?.MapMarker.IconId ?? markerCopy.MapMarker.IconId;
            
            // Get the actual radius value for this marker, typically the circle icon will have this value.
            markerCopy.MapMarker.Scale = group.Max(marker => marker.MapMarker.Scale);
        
            markerCopy.Draw(DrawPosition, Scale);
        }
    }
}