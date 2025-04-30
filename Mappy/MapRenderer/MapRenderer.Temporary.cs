using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Extensions;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    // Note, unlike EventMarkers, Temporary Markers seem to layer the icons in reverse.
    private unsafe void DrawTemporaryMarkers() {
        if (AgentMap.Instance()->SelectedMapSub != AgentMap.Instance()->SelectedMapId) return;
        
        var correctlySizedSpan = AgentMap.Instance()->TempMapMarkers;
        
        for (var index = 0; index < AgentMap.Instance()->TempMapMarkerCount; ++index) {
            var marker = correctlySizedSpan[index];

            // If there's at least one more marker after this
            if (index < AgentMap.Instance()->TempMapMarkerCount - 1) {
                var nextMarker = correctlySizedSpan[index + 1];
                var currentPosition = new Vector2(marker.MapMarker.X, marker.MapMarker.Y);
                var nextPosition = new Vector2(nextMarker.MapMarker.X, nextMarker.MapMarker.Y);
                
                // Check if the next marker matches our position, and that the next marker has a radius, and we do not.
                if (currentPosition == nextPosition && marker.MapMarker.Scale <= 1.0f && nextMarker.MapMarker.Scale > 1.0f) {
                    // to cover cosmic exploration missions that do not set a icon id
                    if (marker.MapMarker.IconId == 0)
                        marker.MapMarker.IconId = 60492;
                    
                    // Set the next marker's radius (writing to a copy of the marker, not mutating the original data)
                    marker.MapMarker.Scale = nextMarker.MapMarker.Scale;
                    
                    // Skip drawing the circle marker, and1584. instead just draw the icon marker, with a correct radius.
                    marker.Draw(DrawPosition, Scale);
                    
                    // Now that we have drawn out of order, we need to skip 2 markers.
                    index++; // Skip the next Marker
                    continue; // Skip this marker
                }
            }
            
            marker.Draw(DrawPosition, Scale);
        }
    }
}