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

    protected override void DrawMarkers(Viewport viewport, Map map) {
        var data = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        
        DrawMapMarkerContainer(data->GuildLeveAssignmentMapMarkerData, viewport, map);
        DrawMapMarkerContainer(data->GuildOrderGuideMarkerData, viewport, map);
        DrawMapMarkerContainer(data->GemstoneTraderMarkerData, viewport, map);
        
        DrawCustomTalkMarkers(viewport, map);
        DrawTripleTriadMarkers(viewport, map);
    }
    
    private void DrawTripleTriadMarkers(Viewport viewport, Map map) {
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
                
                DrawObjective(subLocation, viewport, map, subLocation.TooltipString->ToString(), cardRewardCache[subLocation.ObjectiveId]);
            }
        }
    }
    
    private void DrawMapMarkerContainer(MapMarkerContainer container, Viewport viewport, Map map) {
        foreach (var markerInfo in container.GetAllMarkers()) {
            foreach (var subLocation in markerInfo.MarkerData.Span) {
                DrawObjective(subLocation, viewport, map, subLocation.TooltipString->ToString());
            }
        }
    }
    
    private void DrawCustomTalkMarkers(Viewport viewport, Map map) {
        var data = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        
        foreach (var markerInfo in data->CustomTalkMarkerData.GetAllMarkers()) {
            foreach (var subLocation in markerInfo.MarkerData.Span) {
                if(markerInfo.Label.ToString().IsNullOrEmpty() && subLocation.IconId is not 61731) continue;
                
                DrawObjective(subLocation, viewport, map, subLocation.TooltipString->ToString());
            }
        }
    }
    
    private void DrawObjective(MapMarkerData markerInfo, Viewport viewport, Map map, string tooltip, string? secondaryTooltip = null) {
        if (LuminaCache<Level>.Instance.GetRow(markerInfo.LevelId) is not { } levelData) return;
        if (levelData.Map.Row != map.RowId) return;
        
        var config = GetConfig<MiscConfig>();

        DrawUtilities.DrawMapIcon(new MappyMapIcon {
            IconId = markerInfo.IconId is 60091 ? 61731 : markerInfo.IconId,
            ObjectPosition = new Vector2(markerInfo.X, markerInfo.Z),
            
            Tooltip = tooltip,
            TooltipExtraText = secondaryTooltip ?? string.Empty,
        }, config, viewport, map);
    }
}