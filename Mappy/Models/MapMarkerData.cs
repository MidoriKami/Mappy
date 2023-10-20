using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using DailyDuty.System;
using Dalamud.Interface;
using Dalamud.Utility;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;
using Mappy.Models.ModuleConfiguration;
using Mappy.System;
using Mappy.System.Localization;
using Mappy.Utility;
using Action = System.Action;

namespace Mappy.DataModels;

public enum MapMarkerType {
    Standard,
    MapLink,
    InstanceLink,
    Aetheryte,
    Aethernet
}

// Todo: Basically, remove this.
public class MapMarkerData {
    private readonly MapIconConfig settings;
    
    private readonly MapMarker data;
    private Vector2 Position => new(data.X, data.Y);
    private PlaceName PlaceName => LuminaCache<PlaceName>.Instance.GetRow(data.PlaceNameSubtext.Row)!;
    private Map DataMap => LuminaCache<Map>.Instance.GetRow(data.DataKey)!;
    private Aetheryte DataAetheryte => LuminaCache<Aetheryte>.Instance.GetRow(data.DataKey)!;
    private PlaceName DataPlaceName => LuminaCache<PlaceName>.Instance.GetRow(data.DataKey)!;
    private byte DataType => data.DataType;
    public MapMarkerType Type => (MapMarkerType) data.DataType;
    public uint IconId => data.Icon;

    private static readonly Dictionary<uint, string> MiscIconNameCache = new();

    public MapMarkerData(MapMarker marker, MapIconConfig config) {
        data = marker;
        settings = config;
    }

    public void Draw(Viewport viewport, Map map) {
        if (IconId is > 063200 and < 63900 || map.Id.RawString.StartsWith("world")) {
            DrawUtilities.DrawMapIcon(new MappyMapIcon {
                IconId = IconId,
                TexturePosition = Position,
                ColorManipulation = new Vector4(0.75f, 0.75f, 0.75f, 1.0f),
            }, settings, viewport, map);
            
            DrawUtilities.DrawMapText(new MappyMapText {
                Text = PlaceName.Name.ToDalamudString().TextValue,
                TexturePosition = Position,
                UseLargeFont = true,
                
                TextColor = KnownColor.Black.Vector(),
                OutlineColor = KnownColor.White.Vector(),
                
                HoverColor = KnownColor.White.Vector(),
                HoverOutlineColor = KnownColor.RoyalBlue.Vector(),
                
                OnClick = GetClickAction(),
                
            }, viewport, map);
        } else {
            DrawUtilities.DrawMapIcon(new MappyMapIcon {
                IconId = IconId,
                TexturePosition = Position,
            
                GetTooltipFunc = GetTooltipString,
                GetTooltipExtraTextFunc = settings.ShowTeleportCostTooltips ? GetSecondaryTooltipString : () => string.Empty,
                GetTooltipColorFunc = GetDisplayColor,
            
                OnClickAction = GetClickAction(),
            
            }, settings, viewport, map);
        }
    }

    private string GetTooltipString() {
        if (GetDisplayString() is null && settings.ShowMiscTooltips) {
            if (!MiscIconNameCache.ContainsKey(IconId)) {
                if (LuminaCache<MapSymbol>.Instance.FirstOrDefault(symbol => symbol.Icon == IconId) is { PlaceName.Value.Name.RawString: var name }) {
                    MiscIconNameCache.Add(IconId, name);
                }
            }

            return MiscIconNameCache.TryGetValue(IconId, out var value) ? value : string.Empty;
        } else if (GetDisplayString() is { } displayString) {
            return displayString;
        }

        return string.Empty;
    }
    
    private string GetSecondaryTooltipString() => (MapMarkerType?) DataType switch {
        MapMarkerType.Standard => string.Empty,
        MapMarkerType.MapLink => string.Empty,
        MapMarkerType.InstanceLink => string.Empty,
        MapMarkerType.Aetheryte => $"{Strings.TeleportCost}: {Service.AetheryteList.FirstOrDefault(entry => entry.AetheryteId == data.DataKey)?.GilCost ?? 0:n0} {Strings.gil}",
        MapMarkerType.Aethernet => string.Empty,
        _ => string.Empty
    };

    private Action? GetClickAction() => (MapMarkerType?) DataType switch {
        MapMarkerType.Standard => null,
        MapMarkerType.MapLink => MapLinkAction,
        MapMarkerType.InstanceLink => null,
        MapMarkerType.Aetheryte => AetheryteAction,
        MapMarkerType.Aethernet => AethernetAction,
        _ => null
    };

    private void MapLinkAction() => MappySystem.MapTextureController.LoadMap(DataMap.RowId);
    private void AetheryteAction() => TeleporterController.Instance.Teleport(DataAetheryte);
    private void AethernetAction() {
        if (LuminaCache<Aetheryte>.Instance.FirstOrDefault(aetheryte => aetheryte.AethernetName.Row == DataPlaceName.RowId) is not { AethernetGroup: var aethernetGroup }) return;
        if (LuminaCache<Aetheryte>.Instance.FirstOrDefault(aetheryte => aetheryte.IsAetheryte && aetheryte.AethernetGroup == aethernetGroup) is not { } targetAetheryte) return;

        TeleporterController.Instance.Teleport(targetAetheryte);
    }
    
    private string? GetDisplayString() => (MapMarkerType) DataType switch {
        MapMarkerType.Standard => GetStandardMarkerString(),
        MapMarkerType.MapLink => PlaceName.Name.ToDalamudString().TextValue,
        MapMarkerType.InstanceLink => null,
        MapMarkerType.Aetheryte => DataAetheryte.PlaceName.Value?.Name.ToDalamudString().TextValue,
        MapMarkerType.Aethernet => DataPlaceName.Name.ToDalamudString().TextValue,
        _ => null
    };

    private Vector4 GetDisplayColor() => (MapMarkerType) DataType switch {
        MapMarkerType.Standard => settings.TooltipColor,
        MapMarkerType.MapLink => settings.MapLinkColor,
        MapMarkerType.InstanceLink => settings.InstanceLinkColor,
        MapMarkerType.Aetheryte => settings.AetheryteColor,
        MapMarkerType.Aethernet => settings.AethernetColor,
        _ => settings.TooltipColor
    };

    private string? GetStandardMarkerString() {
        var placeName = PlaceName.Name.ToDalamudString().TextValue;
        if (placeName != string.Empty) return placeName;

        var mapSymbol = LuminaCache<MapSymbol>.Instance.GetRow(data.Icon);
        return mapSymbol?.PlaceName.Value?.Name.ToDalamudString().TextValue;
    }
}