using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Extensions;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    private unsafe void DrawDynamicMarkers() {
        // Skip drawing dynamic markers if we are in a housing district, we need to process those a little different.
        if (HousingManager.Instance()->CurrentTerritory is not null) return;

        // Group together icons based on their dataId, this is because square enix shows circles then draws the actual icon overtop
        var iconGroups = AgentMap.Instance()->EventMarkers.GroupBy(markers => markers.DataId);

        foreach (var group in iconGroups) {
            // Make a copy of the first marker in the set, we will be mutating this copy.
            var markerCopy = group.First();

            // Get the actual iconId we want, typically the icon for the fate, not the circle.
            markerCopy.IconId = group.First(marker => marker.IconId is not 60493).IconId;
            
            // Get the actual radius value for this marker, typically the circle icon will have this value.
            markerCopy.Radius = group.Max(marker => marker.Radius);

            markerCopy.Draw(DrawPosition, Scale);
        }
    }
}