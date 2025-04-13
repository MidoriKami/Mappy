using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.String;
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
            PrimaryText = () => GetMarkerPrimaryText(marker),
            IsDynamicMarker = true,
            ObjectiveId = marker.ObjectiveId,
            LevelId = marker.LevelId,
            MarkerType = GetMarkerType(marker),
            DataId = GetMarkerDataId(marker),
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
            PrimaryText = () => GetMarkerPrimaryText(marker),
            IsDynamicMarker = true,
            ObjectiveId = marker.ObjectiveId,
            LevelId = marker.LevelId,
            MarkerType = GetMarkerType(marker),
            DataId = GetMarkerDataId(marker),
        }, text);
    }

    private static unsafe string GetMarkerPrimaryText(MapMarkerData marker) {
        if (marker.TooltipString is null) return string.Empty;
        if (marker.TooltipString->StringPtr.Value is null) return string.Empty;
        if (marker.TooltipString->StringPtr.ExtractText().IsNullOrEmpty()) return string.Empty;
        
        var text = marker.TooltipString->StringPtr.ExtractText();
        return marker.RecommendedLevel is 0 ? text : $"Lv. {text}";
    }

    private static unsafe MarkerType GetMarkerType(MapMarkerData marker)
        => (MarkerType)((ExtendedMapMarkerData*) (&marker))->MarkerType;

    private static unsafe ushort GetMarkerDataId(MapMarkerData marker)
        => ((ExtendedMapMarkerData*) (&marker))->DataId;
}

[StructLayout(LayoutKind.Explicit, Size = 0x50)]
public unsafe struct ExtendedMapMarkerData {
    [FieldOffset(0x00)] public uint LevelId;
    [FieldOffset(0x04)] public uint ObjectiveId;
    [FieldOffset(0x08)] public Utf8String* TooltipString;
    [FieldOffset(0x10)] public uint IconId;

    [FieldOffset(0x1C)] public float X;
    [FieldOffset(0x20)] public float Y;
    [FieldOffset(0x24)] public float Z;
    [FieldOffset(0x28)] public float Radius;

    [FieldOffset(0x30)] public uint MapId;
    [FieldOffset(0x34)] public uint PlaceNameZoneId;
    [FieldOffset(0x38)] public uint PlaceNameId;

    [FieldOffset(0x40)] public ushort RecommendedLevel;
    [FieldOffset(0x42)] public ushort TerritoryTypeId;

    [FieldOffset(0x44)] public ushort DataId;
    [FieldOffset(0x46)] public byte MarkerType;
}