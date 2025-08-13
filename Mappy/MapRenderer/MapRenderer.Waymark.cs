using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Classes;
using FieldMarker = Lumina.Excel.Sheets.FieldMarker;
using MarkerInfo = Mappy.Classes.MarkerInfo;

namespace Mappy.MapRenderer;

public partial class MapRenderer
{
    private readonly List<FieldMarker> fieldMarkers = Service.DataManager.GetExcelSheet<FieldMarker>().Where(marker => marker.MapIcon is not 0).ToList();

    private unsafe void DrawFieldMarkers()
    {
        if (AgentMap.Instance()->CurrentMapId != AgentMap.Instance()->SelectedMapId) return;
        var markerSpan = MarkingController.Instance()->FieldMarkers;

        foreach (var index in Enumerable.Range(0, 8)) {
            if (markerSpan[index] is { Active: true } marker) {
                var markerPosition =
                    new Vector2(marker.X, marker.Z) / 1000.0f * DrawHelpers.GetMapScaleFactor()
                    - DrawHelpers.GetMapOffsetVector()
                    + DrawHelpers.GetMapCenterOffsetVector();

                DrawHelpers.DrawMapMarker(new MarkerInfo
                {
                    Offset = DrawPosition,
                    Scale = Scale,
                    Position = markerPosition * Scale,
                    IconId = fieldMarkers[index].MapIcon,
                });
            }
        }
    }
}