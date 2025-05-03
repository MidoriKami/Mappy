using System.Numerics;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Mappy.Classes;
using MarkerInfo = Mappy.Classes.MarkerInfo;

namespace Mappy.Extensions;

// MapMarkerData struct represents dynamic markers that have information like radius, and other fields.
public static class MapMarkerDataExtensions {
    public static void Draw(this MapMarkerData marker, Vector2 offset, float scale) {
        if ((marker.Flags & 1) == 1) return;
        
        DrawHelpers.DrawMapMarker(new MarkerInfo {
            Position = (marker.Position.AsMapVector() * DrawHelpers.GetMapScaleFactor() - DrawHelpers.GetMapOffsetVector() + DrawHelpers.GetMapCenterOffsetVector()) * scale,
            Offset = offset,
            Scale = scale,
            IconId = marker.IconId,
            Radius = marker.Radius,
            RadiusColor = System.SystemConfig.AreaColor,
            RadiusOutlineColor = System.SystemConfig.AreaOutlineColor,
            PrimaryText = () => GetMarkerPrimaryText(marker),
            IsDynamicMarker = true,
            ObjectiveId = marker.ObjectiveId,
            LevelId = marker.LevelId,
            MarkerType = (MarkerType) marker.MarkerType,
            DataId = marker.DataId,
        });
    }

    private static unsafe string GetMarkerPrimaryText(MapMarkerData marker) {
        if (marker.TooltipString is null) return string.Empty;
        if (marker.TooltipString->StringPtr.Value is null) return string.Empty;
        if (marker.TooltipString->StringPtr.ExtractText().IsNullOrEmpty()) return string.Empty;
        
        var text = marker.TooltipString->StringPtr.ExtractText();
        return marker.RecommendedLevel is 0 ? text : $"Lv. {marker.RecommendedLevel} {text}";
    }
}
