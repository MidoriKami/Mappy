using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Classes;
using MarkerInfo = Mappy.Classes.MarkerInfo;

namespace Mappy.Extensions;

// MapMarkerData struct represents dynamic markers that have information like radius, and other fields.
public static class MapMarkerDataExtensions {
    public static unsafe void Draw(this MapMarkerData marker, Vector2 offset, float scale) {
        DrawHelpers.DrawMapMarker(new MarkerInfo {
            Position = (new Vector2(marker.X, marker.Z) * AgentMap.Instance()->SelectedMapSizeFactorFloat + new Vector2(1024.0f, 1024.0f)) * scale,
            Offset = offset,
            Scale = scale,
            IconId = marker.IconId,
            Radius = marker.Radius,
            PrimaryText = () => marker.RecommendedLevel is 0 ? marker.TooltipString->ToString() : $"Lv. {marker.RecommendedLevel} {marker.TooltipString->ToString()}",
            IsDynamicMarker = true,
            ObjectiveId = marker.ObjectiveId,
            LevelId = marker.LevelId,
        });
    }
}