using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Mappy.Classes;
using FieldMarker = Lumina.Excel.GeneratedSheets.FieldMarker;
using MarkerInfo = Mappy.Classes.MarkerInfo;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    private readonly List<FieldMarker> fieldMarkers = Service.DataManager.GetExcelSheet<FieldMarker>()!.Where(marker => marker.MapIcon is not 0).ToList();

    private unsafe void DrawFieldMarkers() {
        var markerSpan = MarkingController.Instance()->FieldMarkerArraySpan;
        
        foreach (var index in Enumerable.Range(0, 8)) {
            if (markerSpan[index] is { Active: true } marker) {
                
                DrawHelpers.DrawMapMarker(new MarkerInfo {
                    Offset = DrawPosition,
                    Scale = Scale,
                    Position = (new Vector2(marker.X, marker.Z) / 1000.0f + new Vector2(1024.0f, 1024.0f)) * Scale,
                    IconId = fieldMarkers[index].MapIcon,
                });
            }
        }
    }
}