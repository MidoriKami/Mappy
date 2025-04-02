using System.Numerics;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Mappy.Classes;
using MarkerInfo = Mappy.Classes.MarkerInfo;

namespace Mappy.Extensions;

// MapMarkerData struct represents dynamic markers that have information like radius, and other fields.
public static class MapMarkerDataExtensions {
    public static unsafe void Draw(this MapMarkerData marker, Vector2 offset, float scale) {
        DrawHelpers.DrawMapMarker(new MarkerInfo {
            Position = (new Vector2(marker.X, marker.Z) * DrawHelpers.GetMapScaleFactor() - DrawHelpers.GetMapOffsetVector() + DrawHelpers.GetMapCenterOffsetVector()) * scale,
            Offset = offset,
            Scale = scale,
            IconId = marker.IconId,
            Radius = marker.Radius,
            RadiusColor = System.SystemConfig.AreaColor,
            RadiusOutlineColor = System.SystemConfig.AreaOutlineColor,
            PrimaryText = () => marker.RecommendedLevel is 0 ? marker.TooltipString->StringPtr.AsReadOnlySeStringSpan().ExtractText() : $"Lv. {marker.RecommendedLevel} {marker.TooltipString->StringPtr.AsReadOnlySeStringSpan().ExtractText()}",
            IsDynamicMarker = true,
            ObjectiveId = marker.ObjectiveId,
            LevelId = marker.LevelId,
        });
    }

    public static unsafe void DrawText(this MapMarkerData marker, string text, Vector2 offset, float scale) {
        DrawHelpers.DrawText(new MarkerInfo {
            Position = (new Vector2(marker.X, marker.Z) * DrawHelpers.GetMapScaleFactor() - DrawHelpers.GetMapOffsetVector() + DrawHelpers.GetMapCenterOffsetVector()) * scale,
            Offset = offset,
            Scale = scale,
            IconId = marker.IconId,
            Radius = marker.Radius,
            RadiusColor = System.SystemConfig.AreaColor,
            RadiusOutlineColor = System.SystemConfig.AreaOutlineColor,
            PrimaryText = () => marker.RecommendedLevel is 0 ? marker.TooltipString->StringPtr.AsReadOnlySeStringSpan().ExtractText() : $"Lv. {marker.RecommendedLevel} {marker.TooltipString->StringPtr.AsReadOnlySeStringSpan().ExtractText()}",
            IsDynamicMarker = true,
            ObjectiveId = marker.ObjectiveId,
            LevelId = marker.LevelId,
        }, text);
    }
}