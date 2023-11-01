using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;
using Mappy.Utility;
using Map = Lumina.Excel.GeneratedSheets.Map;

namespace Mappy.System.Modules;

public unsafe class MiscMarkers : ModuleBase {
    public override ModuleName ModuleName => ModuleName.MiscMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new MiscConfig();
    private readonly Dictionary<uint, string> cardRewardCache = new();

    protected override void UpdateMarkers(Viewport viewport, Map map) {
        var data = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        
        data->GuildLeveAssignments.DrawMarkers(DrawMarker, map);
        data->GuildOrderGuides.DrawMarkers(DrawMarker, map);
        data->GemstoneTraders.DrawMarkers(DrawMarker, map);
        data->CustomTalk.DrawMarkers(DrawMarker, map, null, FilterCustomTalk);
        data->TripleTriad.DrawMarkers(DrawMarker, map, TripleTriadSecondaryText);
    }

    private string TripleTriadSecondaryText(MapMarkerData marker) {
        if (LuminaCache<TripleTriad>.Instance.GetRow(marker.ObjectiveId) is not { } triadInfo) return string.Empty;

        if (!cardRewardCache.ContainsKey(marker.ObjectiveId)) {
            var cardRewards = triadInfo.ItemPossibleReward
                .Where(reward => reward.Row is not 0)
                .Select(reward => reward.Value)
                .OfType<Item>()
                .Select(item => item.Name.RawString);
            
            cardRewardCache.Add(marker.ObjectiveId, string.Join("\n", cardRewards));
        }
        
        return cardRewardCache.TryGetValue(marker.ObjectiveId, out var cards) ? cards : string.Empty;
    }

    private bool FilterCustomTalk(MapMarkerData marker) 
        // Only allow markers that are empty, if they are also icon id 61731, used in IslandSanctuary
        => marker.TooltipString->ToString().IsNullOrEmpty() && marker.IconId is not 61731;

    private void DrawMarker(MapMarkerData marker, Map map, Func<MapMarkerData, string>? tooltipExtraText = null, Func<object>? arg5 = null) {
        if (marker.MapId != map.RowId) return;

        UpdateIcon((marker.ObjectiveId, marker.LevelId, marker.IconId), () => new MappyMapIcon {
            MarkerId = (marker.ObjectiveId, marker.LevelId, marker.IconId),
            IconId = marker.IconId is 60091 ? 61731 : marker.IconId,
            ObjectPosition = new Vector2(marker.X, marker.Z),
            Tooltip = marker.TooltipString->ToString(),
            TooltipExtraText = tooltipExtraText is not null ? tooltipExtraText.Invoke(marker) : string.Empty,
        });
    }
}