﻿using System.Linq;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Extensions;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    private unsafe void DrawStaticMapMarkers() {
        foreach (var index in Enumerable.Range(0, AgentMap.Instance()->MapMarkerCount)) {
            ref var marker = ref AgentMap.Instance()->MapMarkers[index];

            if (marker.MapMarker.IconId is 0) continue;
            
            marker.Draw(DrawPosition, Scale);
        }
    }

    private unsafe void DrawStaticTextMarkers() {
        foreach (var index in Enumerable.Range(0, AgentMap.Instance()->MapMarkerCount)) {
            ref var marker = ref AgentMap.Instance()->MapMarkers[index];
            
            if (marker.MapMarker.IconId is not 0) continue;
            if (marker.MapMarker.Index is 0) continue;
            if (marker.MapMarker.SubtextOrientation is 0) continue;

            marker.Draw(DrawPosition, Scale);
        }
    }
}