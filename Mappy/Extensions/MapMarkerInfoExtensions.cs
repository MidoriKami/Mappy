﻿using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Memory;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;
using Mappy.Classes;

namespace Mappy.Extensions;

public static class MapMarkerInfoExtensions {
    public  static unsafe void Draw(this MapMarkerInfo marker, Vector2 offset, float scale) {
        var tooltipText = MemoryHelper.ReadSeStringNullTerminated((nint) marker.MapMarker.Subtext);
        
        DrawHelpers.DrawMapMarker(new MarkerInfo {
            // Divide by 16, as it seems they use a fixed scalar
            // Add 1024 * scale, to offset from top-left, to center-based coordinate
            // Add offset for drawing relative to map when its moved around
            Position = new Vector2(marker.MapMarker.X, marker.MapMarker.Y) / 16.0f * scale + DrawHelpers.GetMapCenterOffsetVector() * scale,
            Offset = offset,
            Scale = scale,
            Radius = marker.MapMarker.Scale,
            RadiusColor = KnownColor.MediumPurple.Vector(),
            IconId = marker.MapMarker.IconId,
            PrimaryText = () => tooltipText.TextValue.IsNullOrEmpty() && System.SystemConfig.ShowMiscTooltips ? System.TooltipCache.GetValue(marker.MapMarker.IconId) : tooltipText.ToString(),
            OnLeftClicked = marker.DataType switch {
                1 => () => System.IntegrationsController.OpenMap(marker.DataKey),
                3 => () => System.Teleporter.Teleport(Service.DataManager.GetExcelSheet<Aetheryte>()!.GetRow(marker.DataKey)!), // Gonna assume that can't be null, because it's a row index that comes from the active gamestate.
                4 when GetAetheryteForAethernet(marker.DataKey) is not null => () => System.Teleporter.Teleport(GetAetheryteForAethernet(marker.DataKey)!),
                _ => null,
            },
            SecondaryText = marker.DataType switch {
                1 => () => $"Open Map {Service.DataManager.GetExcelSheet<Map>()!.GetRow(marker.DataKey)?.PlaceName.Value?.Name ?? "Unable to read target map name."}",
                2 => () => $"Instance Link? {marker.DataKey}",
                3 => () => $"Teleport to {Service.DataManager.GetExcelSheet<Aetheryte>()!.GetRow(marker.DataKey)?.PlaceName.Value?.Name ?? "Unable to read aetheryte name"} {GetAetheryteTeleportCost(marker.DataKey)}",
                4 when GetAetheryteForAethernet(marker.DataKey) is not null => () => $"Teleport to {GetAetheryteForAethernet(marker.DataKey)?.PlaceName.Value?.Name ?? "Unable to read aetheryte name"} {GetAetheryteTeleportCost(GetAetheryteForAethernet(marker.DataKey)!.RowId)}",
                _ => null
            }
        });
    }

    // Might want to cache these in the future, they are kinda expensive.
    private static Aetheryte? GetAetheryteForAethernet(uint aethernetKey) {
        if (Service.DataManager.GetExcelSheet<Aetheryte>()!.FirstOrDefault(aetheryte => aetheryte.AethernetName.Row == aethernetKey) is not { AethernetGroup: var aethernetGroup }) return null;
        if (Service.DataManager.GetExcelSheet<Aetheryte>()!.FirstOrDefault(aetheryte => aetheryte.IsAetheryte && aetheryte.AethernetGroup == aethernetGroup) is not { } targetAetheryte) return null;

        return targetAetheryte;
    }

    // Might want to cache these in the future, they are kinda expensive.
    private static string GetAetheryteTeleportCost(uint targetDataKey) 
        => $"({Service.AetheryteList.FirstOrDefault(entry => entry.AetheryteId == targetDataKey)?.GilCost ?? 0:n0} {SeIconChar.Gil.ToIconChar()})";
}