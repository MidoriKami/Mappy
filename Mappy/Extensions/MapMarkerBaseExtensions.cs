using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Memory;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Classes;

namespace Mappy.Extensions;

// Represents standard non-dynamic map markers, things that don't change, and may reference datasheet data with their key data
public static class MapMarkerBaseExtensions {
    public static unsafe void Draw(this MapMarkerBase marker, Vector2 offset, float scale) {
        var tooltipText = MemoryHelper.ReadSeStringNullTerminated((nint) marker.Subtext);
        
        DrawHelpers.DrawMapMarker(new MarkerInfo {
            // Divide by 16, as it seems they use a fixed scalar
            // Add 1024 * scale, to offset from top-left, to center-based coordinate
            // Add offset for drawing relative to map when its moved around
            Position = new Vector2(marker.X, marker.Y) / 16.0f * scale + DrawHelpers.GetMapCenterOffsetVector() * scale,
            Offset = offset,
            Scale = scale,
            Radius = marker.Scale,
            RadiusColor = KnownColor.MediumPurple.Vector(),
            IconId = marker.IconId,
            PrimaryText = () => tooltipText.TextValue.IsNullOrEmpty() && System.SystemConfig.ShowMiscTooltips ? System.TooltipCache.GetValue(marker.IconId) : tooltipText.ToString(),
        });
    }
}