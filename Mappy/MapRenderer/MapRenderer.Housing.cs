using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Extensions;

namespace Mappy.MapRenderer;

public unsafe partial class MapRenderer {
    private void DrawHousingMarkers() {
        if (HousingManager.Instance()->CurrentTerritory is not null) {
            var markers = AgentMap.Instance()->EventMarkers;

            foreach (var index in Enumerable.Range(0, markers.Count)) {
                var marker = markers[index];

                marker.Radius = 0.0f;
                marker.Draw(DrawPosition, Scale);
                
                if (IsNotHouseMarker(marker.IconId)) continue;
                marker.DrawText($"{index + 1}", DrawPosition, Scale);
            }
        }
    }

    private bool IsNotHouseMarker(uint iconId) => iconId switch {
        >= 60789 and < 60800 => true,
        _ => false,
    };
}