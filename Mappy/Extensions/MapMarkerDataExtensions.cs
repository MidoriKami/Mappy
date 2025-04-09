using System.Numerics;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Mappy.Classes;
using MarkerInfo = Mappy.Classes.MarkerInfo;

namespace Mappy.Extensions;

// MapMarkerData struct represents dynamic markers that have information like radius, and other fields.
public static class MapMarkerDataExtensions {
    public static void Draw(this MapMarkerData marker, Vector2 offset, float scale) {
        DrawHelpers.DrawMapMarker(new MarkerInfo {
            Position = (new Vector2(marker.X, marker.Z) * DrawHelpers.GetMapScaleFactor() - DrawHelpers.GetMapOffsetVector() + DrawHelpers.GetMapCenterOffsetVector()) * scale,
            Offset = offset,
            Scale = scale,
            IconId = marker.IconId,
            Radius = marker.Radius,
            RadiusColor = System.SystemConfig.AreaColor,
            RadiusOutlineColor = System.SystemConfig.AreaOutlineColor,
            PrimaryText = () => GetMarkerPrimaryText(ref marker),
            IsDynamicMarker = true,
            ObjectiveId = marker.ObjectiveId,
            LevelId = marker.LevelId,
        });
    }

    public static void DrawText(this MapMarkerData marker, string text, Vector2 offset, float scale) {
        DrawHelpers.DrawText(new MarkerInfo {
            Position = (new Vector2(marker.X, marker.Z) * DrawHelpers.GetMapScaleFactor() - DrawHelpers.GetMapOffsetVector() + DrawHelpers.GetMapCenterOffsetVector()) * scale,
            Offset = offset,
            Scale = scale,
            IconId = marker.IconId,
            Radius = marker.Radius,
            RadiusColor = System.SystemConfig.AreaColor,
            RadiusOutlineColor = System.SystemConfig.AreaOutlineColor,
            PrimaryText = () => GetMarkerPrimaryText(ref marker),
            IsDynamicMarker = true,
            ObjectiveId = marker.ObjectiveId,
            LevelId = marker.LevelId,
        }, text);
    }

    private static unsafe string GetMarkerPrimaryText(ref MapMarkerData marker) {
        if (marker.TooltipString is null) return "Error Parsing Marker Text\nTooltipString is null";
        if (marker.TooltipString->StringPtr.Value is null) return "Error Parsing Marker Text\nTooltipString.Value is null";
        if (marker.TooltipString->StringPtr.ExtractText().IsNullOrEmpty()) return "Error Parsing Marker Text\nExtracted Text is null or empty.";
        
        var text = marker.TooltipString->StringPtr.ExtractText();
        return marker.RecommendedLevel is 0 ? text : $"Lv. {text}";
    }
}