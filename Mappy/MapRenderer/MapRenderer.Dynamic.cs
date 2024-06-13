using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Extensions;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    private unsafe void DrawDynamicMarkers() {
        for (var index = 0; index < AgentMap.Instance()->EventMarkers.Count; ++index) {
            var marker = AgentMap.Instance()->EventMarkers[index];
            if (marker.IconId is 0) continue;

            // If there's at least one more marker after this
            if (index < AgentMap.Instance()->EventMarkers.Count - 1) {
                var nextMarker = AgentMap.Instance()->EventMarkers[index + 1];
                var currentPosition = new Vector3(marker.X, marker.Y, marker.Z);
                var nextPosition = new Vector3(nextMarker.X, nextMarker.Y, nextMarker.Z);
                
                // Check if the next marker matches our position, and that we have a radius, and the next marker does not.
                if (currentPosition == nextPosition && marker.Radius > 1.0f && nextMarker.Radius <= 1.0f) {
                    
                    // Set the next marker's radius (writing to a copy of the marker, not mutating the original data)
                    nextMarker.Radius = marker.Radius;
                    
                    // Skip drawing the circle marker, and1584. instead just draw the icon marker, with a correct radius.
                    nextMarker.Draw(DrawPosition, Scale);
                    
                    // Now that we have drawn out of order, we need to skip 2 markers.
                    index++; // Skip the next Marker
                    continue; // Skip this marker
                }
            }
            
            marker.Draw(DrawPosition, Scale);
        }
    }
}