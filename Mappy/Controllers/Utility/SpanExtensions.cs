using System;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.STD;
using Map = Lumina.Excel.GeneratedSheets.Map;

namespace Mappy.Utility; 

public static class SpanExtensions {
    public static void DrawMarkers(this Span<MarkerInfo> span,         
        Action<MapMarkerData, Map, Func<MapMarkerData, string>?, Func<object>?> drawMarker, 
        Map map, 
        Func<MapMarkerData, string>? tooltipExtraText = null,
        Func<MapMarkerData, bool>? filterFunction = null,
        Func<MarkerInfo, bool>? parentFilter = null,
        Func<object>? extraData = null) {
        foreach (var markerInfo in span) {
            if (!markerInfo.ShouldRender) continue;
            if (parentFilter is not null) {
                if (parentFilter.Invoke(markerInfo)) {
                    continue;
                }
            }
            
            foreach (var markerData in markerInfo.MarkerData.Span) {
                if (filterFunction is not null) {
                    if (filterFunction.Invoke(markerData)) {
                        continue;
                    }
                }
                
                drawMarker(markerData, map, tooltipExtraText, extraData);
            }
        }
    }

    public static void DrawMarkers(this StdVector<MapMarkerData> vector,
        Action<MapMarkerData, Map, Func<MapMarkerData, string>?, Func<object>?> drawMarker, 
        Map map, 
        Func<MapMarkerData, string>? tooltipExtraText = null,
        Func<MapMarkerData, bool>? filterFunction = null,
        Func<object>? extraData = null) {
        foreach (var markerData in vector.Span) {
            if (filterFunction is not null) {
                if (filterFunction.Invoke(markerData)) {
                    continue;
                }
            }
            
            drawMarker(markerData, map, tooltipExtraText, extraData);
        }
    }
}