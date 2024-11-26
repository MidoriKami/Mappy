using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using Dalamud.Memory;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using Mappy.Classes;
using Map = Lumina.Excel.Sheets.Map;
using MarkerInfo = Mappy.Classes.MarkerInfo;

namespace Mappy.Extensions;

public static class MapMarkerInfoExtensions {
    public  static unsafe void Draw(this MapMarkerInfo marker, Vector2 offset, float scale) {
        var tooltipText = MemoryHelper.ReadSeStringNullTerminated((nint) marker.MapMarker.Subtext);
        
        DrawHelpers.DrawMapMarker(new MarkerInfo {
            // Divide by 16, as it seems they use a fixed scalar
            // Add 1024 * scale, to offset from top-left, to center-based coordinate
            // Add offset for drawing relative to map when it's moved around
            Position = new Vector2(marker.MapMarker.X, marker.MapMarker.Y) / 16.0f * scale + DrawHelpers.GetMapCenterOffsetVector() * scale,
            Offset = offset,
            Scale = scale,
            Radius = marker.MapMarker.Scale,
            RadiusColor = KnownColor.MediumPurple.Vector(),
            IconId = marker.MapMarker.IconId,
            PrimaryText = GetMarkerPrimaryTooltip(marker, tooltipText),
            OnLeftClicked = marker.DataType switch {
                1 when !DrawHelpers.IsDisallowedIcon(marker.MapMarker.IconId) => () => System.IntegrationsController.OpenMap(marker.DataKey),
                3 => () => System.Teleporter.Teleport(marker.DataKey),
                4 when GetAetheryteForAethernet(marker.DataKey) is {} aetheryte => () => System.Teleporter.Teleport(aetheryte.RowId),
                _ => null,
            },
            SecondaryText = marker.DataType switch {
                1 when !DrawHelpers.IsDisallowedIcon(marker.MapMarker.IconId) => () => $"Open Map {Service.DataManager.GetExcelSheet<Map>().GetRow(marker.DataKey).PlaceName.Value.Name.ExtractText()}",
                2 => () => $"Instance Link {marker.DataKey}",
                3 => () => $"Teleport to {Service.DataManager.GetExcelSheet<Aetheryte>().GetRow(marker.DataKey).PlaceName.Value.Name.ExtractText()} {GetAetheryteTeleportCost(marker.DataKey)}",
                4 when GetAetheryteForAethernet(marker.DataKey) is not null => () => $"Teleport to {GetAetheryteForAethernet(marker.DataKey)!.Value.PlaceName.Value.Name.ExtractText()} {GetAetheryteTeleportCost(GetAetheryteForAethernet(marker.DataKey)!.Value.RowId)}",
                _ => null,
            },
        });
    }

    private static Aetheryte? GetAetheryteForAethernet(uint aethernetKey)
        => System.AetheryteAethernetCache.GetValue(aethernetKey);

    private static string GetAetheryteTeleportCost(uint targetDataKey) 
        => $"({Service.AetheryteList.FirstOrDefault(entry => entry.AetheryteId == targetDataKey)?.GilCost ?? 0:n0} {SeIconChar.Gil.ToIconChar()})";

    private static Func<string> GetMarkerPrimaryTooltip(MapMarkerInfo marker, SeString tooltipText) {
        if (DrawHelpers.IsDisallowedIcon(marker.MapMarker.IconId)) return () => string.Empty;
        if (!System.SystemConfig.ShowMiscTooltips) return () => string.Empty;
        if (!tooltipText.TextValue.IsNullOrEmpty()) return tooltipText.ToString;
        
        return marker.DataType switch {
            4 => () => Service.DataManager.GetExcelSheet<PlaceName>().GetRow(marker.DataKey).Name.ExtractText(),
            _ => () => System.TooltipCache.GetValue(marker.MapMarker.IconId) ?? string.Empty,
        };
    }
}