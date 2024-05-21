using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Classes;

namespace Mappy.Extensions;

public static class TempMapMarkerExtensions {
    public static void Draw(this TempMapMarker marker, Vector2 offset, float scale) {
        DrawHelpers.DrawMapMarker(new MarkerInfo {
            // Divide by 16, as it seems they use a fixed scalar
            // Add 1024 * scale, to offset from top-left, to center-based coordinate
            // Add offset for drawing relative to map when its moved around
            Position = (new Vector2(marker.MapMarker.X, marker.MapMarker.Y) / 16.0f * DrawHelpers.GetMapScaleFactor() + DrawHelpers.GetCombinedOffsetVector()) * scale,
            Offset = offset,
            Scale = scale,
            IconId = marker.MapMarker.IconId,
            Radius = marker.MapMarker.Scale,
            PrimaryText = () => marker.TooltipText.ToString(),
        });
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 0x110)]
    public struct TempMapMarker {
        [FieldOffset(0x00)] public Utf8String TooltipText;
        [FieldOffset(0x68)] public MapMarkerBase MapMarker;

        [FieldOffset(0xA8)] public uint StyleFlags;
        [FieldOffset(0xAC)] public uint Type;
    }
}