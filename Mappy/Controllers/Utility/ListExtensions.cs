using System;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.STD;
using Map = Lumina.Excel.GeneratedSheets.Map;

namespace Mappy.Utility; 

public static class ListExtensions {
    public unsafe ref struct StdListEnumerator<T> where T : unmanaged {
        private ulong currentIndex;
        private readonly StdList<T> items;
        private StdList<T>.Node* current;

        public StdListEnumerator(StdList<T> list) {
            items = list;
            currentIndex = 0;
            current = list.Head;
        }
        
        public bool MoveNext() {
            if (currentIndex < items.Size) {
                if (current is not null && current->Next is not null) {
                    current = current->Next;
                    currentIndex++;
                    return true;
                }
            }

            return false;
        }
        
        public readonly ref T Current => ref current->Value;
        public StdListEnumerator<T> GetEnumerator() => new(items);
    }
    
    public static StdListEnumerator<T> GetEnumerator<T>(this StdList<T> list) where T : unmanaged => new(list);

    public static void DrawMarkers(this StdList<MarkerInfo> list, 
        Action<MapMarkerData, Map, Func<MapMarkerData, string>?, Func<object>?> drawMarker, 
        Map map, 
        Func<MapMarkerData, string>? tooltipExtraText = null,
        Func<MapMarkerData, bool>? filterFunction = null,
        Func<object>? extraData = null) {
        foreach (var markerInfo in list.GetEnumerator()) {
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
}

