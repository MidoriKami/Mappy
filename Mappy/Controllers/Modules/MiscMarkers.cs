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
using Map = Lumina.Excel.GeneratedSheets.Map;

namespace Mappy.System.Modules;

public unsafe class MiscMarkers : ModuleBase {
    public override ModuleName ModuleName => ModuleName.MiscMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new MiscConfig();
    private readonly Dictionary<uint, string> cardRewardCache = new();

    protected override void UpdateMarkers(Viewport viewport, Map map) {
        var data = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        
        DrawMapMarkerContainer(data->GuildLeveAssignmentMapMarkerData, map);
        DrawMapMarkerContainer(data->GuildOrderGuideMarkerData, map);
        DrawMapMarkerContainer(data->GemstoneTraderMarkerData, map);
        
        DrawCustomTalkMarkers(map);
        DrawTripleTriadMarkers(map);
    }
    
    private void DrawTripleTriadMarkers(Map map) {
        var data = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();

        foreach (var markerInfo in data->TripleTriadMarkerData.GetAllMarkers()) {
            foreach (var subLocation in markerInfo.MarkerData.Span) {
                if (!cardRewardCache.ContainsKey(subLocation.ObjectiveId)) {
                    if (LuminaCache<TripleTriad>.Instance.GetRow(subLocation.ObjectiveId) is not { } triadInfo) continue;
                    
                    var cardRewards = triadInfo.ItemPossibleReward
                        .Where(reward => reward.Row is not 0)
                        .Select(reward => reward.Value)
                        .OfType<Item>()
                        .Select(item => item.Name.RawString);
                    
                    cardRewardCache.Add(subLocation.ObjectiveId, string.Join("\n", cardRewards));
                }
                
                DrawObjective(subLocation, map, subLocation.TooltipString->ToString(), cardRewardCache[subLocation.ObjectiveId]);
            }
        }
    }
    
    private void DrawMapMarkerContainer(MapMarkerContainer container, Map map) {
        foreach (var markerInfo in container.GetAllMarkers()) {
            foreach (var subLocation in markerInfo.MarkerData.Span) {
                DrawObjective(subLocation, map, subLocation.TooltipString->ToString());
            }
        }
    }
    
    private void DrawCustomTalkMarkers(Map map) {
        var data = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        
        foreach (var markerInfo in data->CustomTalkMarkerData.GetAllMarkers()) {
            foreach (var subLocation in markerInfo.MarkerData.Span) {
                if(markerInfo.Label.ToString().IsNullOrEmpty() && subLocation.IconId is not 61731) continue;
                
                DrawObjective(subLocation, map, subLocation.TooltipString->ToString());
            }
        }
    }
    
    private void DrawObjective(MapMarkerData markerInfo, Map map, string tooltip, string? secondaryTooltip = null) {
        if (LuminaCache<Level>.Instance.GetRow(markerInfo.LevelId) is not { } levelData) return;
        if (levelData.Map.Row != map.RowId) return;
        
        UpdateIcon((markerInfo.ObjectiveId, markerInfo.LevelId), () => new MappyMapIcon {
            MarkerId = (markerInfo.ObjectiveId, markerInfo.LevelId),
            IconId = markerInfo.IconId is 60091 ? 61731 : markerInfo.IconId,
            ObjectPosition = new Vector2(markerInfo.X, markerInfo.Z),
            Tooltip = tooltip,
            TooltipExtraText = secondaryTooltip ?? string.Empty,
        });
    }
}