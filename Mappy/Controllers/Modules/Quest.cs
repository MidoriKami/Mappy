using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;
using Map = Lumina.Excel.GeneratedSheets.Map;
using QuestLinkMarker = FFXIVClientStructs.FFXIV.Client.UI.Agent.QuestLinkMarker;

namespace Mappy.System.Modules;

public unsafe class Quest : ModuleBase {
    public override ModuleName ModuleName => ModuleName.QuestMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new QuestConfig();

    protected override void UpdateMarkers(Viewport viewport, Map map) {
        var config = GetConfig<QuestConfig>();

        if (!config.HideUnacceptedQuests) DrawUnacceptedQuests(map);
        if (!config.HideAcceptedQuests) DrawAcceptedQuests(map);
        if (!config.HideLeveQuests) DrawLeveQuests(map);
    }

    private void DrawAcceptedQuests(Map map) {
        var mapData = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        var config = GetConfig<QuestConfig>();

        foreach (var quest in mapData->QuestDataSpan) {
            if (!quest.ShouldRender) continue;

            foreach (var questInfo in quest.MarkerData.Span) {
                if (LuminaCache<Level>.Instance.GetRow(questInfo.LevelId) is not { Map.Row: var levelMap, Territory.Row: var levelTerritory }) continue;
                if (levelMap != map.RowId && levelTerritory != map.TerritoryType.Row) continue;

                UpdateIcon((questInfo.ObjectiveId, questInfo.LevelId), () => new MappyMapIcon {
                    MarkerId = (questInfo.ObjectiveId, questInfo.LevelId),
                    IconId = questInfo.IconId,
                    ObjectPosition = new Vector2(questInfo.X, questInfo.Z),
                    Tooltip = quest.Label.ToString(),
                    MinimumRadius = questInfo.Radius,
                    RadiusColor = config.InProgressColor,
                    VerticalPosition = questInfo.Y,
                }, icon => {
                    icon.RadiusColor = config.InProgressColor;
                });
            }
        }

        var questLinkSpan = new ReadOnlySpan<QuestLinkMarker>(AgentMap.Instance()->MiniMapQuestLinkContainer.Markers, AgentMap.Instance()->MiniMapQuestLinkContainer.MarkerCount);
        foreach (var marker in questLinkSpan) {
            if (LuminaCache<Level>.Instance.GetRow(marker.LevelId) is not { X: var x, Y: var y, Z: var z }) continue;
            if (map.RowId != marker.SourceMapId) continue;

            UpdateIcon((marker.QuestId, marker.LevelId), () => new MappyMapIcon {
                MarkerId = (marker.QuestId, marker.LevelId),
                IconId = marker.IconId,
                ObjectPosition = new Vector2(x, z),
                GetTooltipFunc = () => marker.TooltipText.ToString(),
                RadiusColor = config.InProgressColor,
                VerticalPosition = y,
                OnClickAction = () => MappySystem.MapTextureController.LoadMap(marker.TargetMapId),
            }, icon => {
                icon.RadiusColor = config.InProgressColor;
            });
        }
    }
    
    private void DrawUnacceptedQuests(Map map) {
        var mapData = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        var config = GetConfig<QuestConfig>();
        
        foreach (var markerInfo in mapData->QuestMarkerData.GetAllMarkers()) {
            foreach (var markerData in markerInfo.MarkerData.Span) {
                if (LuminaCache<Level>.Instance.GetRow(markerData.LevelId) is not { Map.Row: var levelMap }) continue;
                if (levelMap != map.RowId) continue;

                UpdateIcon((markerData.ObjectiveId, markerData.LevelId), () => new MappyMapIcon {
                    MarkerId = (markerData.ObjectiveId, markerData.LevelId),
                    IconId = markerData.IconId,
                    ObjectPosition = new Vector2(markerData.X, markerData.Z),
                    Tooltip = $"Lv. {markerData.RecommendedLevel} {markerData.TooltipString->ToString()}",
                    MinimumRadius = markerData.Radius,
                    RadiusColor = config.InProgressColor,
                    VerticalPosition = markerData.Y,
                }, icon => {
                    icon.RadiusColor = config.InProgressColor;
                });
            }
        }
    }
    
    private void DrawLeveQuests(Map map) {
        var mapData = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        var config = GetConfig<QuestConfig>();

        foreach (var quest in mapData->LevequestDataSpan) {
            if (!quest.ShouldRender) continue;
            
            foreach (var questInfo in quest.MarkerData.Span) {
                if (GetLevework(quest.ObjectiveId) is not { Flags: not 32 } ) continue;
                if (LuminaCache<Level>.Instance.GetRow(questInfo.LevelId) is not { Map.Row: var levelMap, Territory.Row: var levelTerritory } ) continue;
                if (levelMap != map.RowId && levelTerritory != map.TerritoryType.Row) continue;
                
                UpdateIcon(quest.ObjectiveId, () => new MappyMapIcon {
                    MarkerId = quest.ObjectiveId,
                    IconId = questInfo.IconId,
                    ObjectPosition = new Vector2(questInfo.X, questInfo.Z),
                    Tooltip = quest.Label.ToString(),
                    MinimumRadius = questInfo.Radius,
                    RadiusColor = config.LeveQuestColor,
                    VerticalPosition = questInfo.Y,
                }, icon => {
                    icon.RadiusColor = config.LeveQuestColor;
                });
            }
        }
        
        foreach (var markerInfo in mapData->ActiveLevequestMarkerData.Span) {
            if(LuminaCache<Level>.Instance.GetRow(markerInfo.LevelId) is not { Map.Row: var levelMap, Territory.Row: var levelTerritory } ) continue;
            if (levelMap != map.RowId && levelTerritory != map.TerritoryType.Row) continue;
            
            UpdateIcon((markerInfo.ObjectiveId, markerInfo.LevelId), () => new MappyMapIcon {
                MarkerId = (markerInfo.ObjectiveId, markerInfo.LevelId),
                IconId = markerInfo.IconId,
                TexturePosition = new Vector2(markerInfo.X, markerInfo.Z),
                Tooltip = markerInfo.TooltipString->ToString(),
                MinimumRadius = markerInfo.Radius,
                RadiusColor = config.LeveQuestColor,
                VerticalPosition = markerInfo.Y,
            }, icon => {
                icon.RadiusColor = config.LeveQuestColor;
            });
        }
    }

    private LeveWork? GetLevework(uint id) {
        foreach (var levework in QuestManager.Instance()->LeveQuestsSpan) {
            if (levework.LeveId == id) return levework;
        }

        return null;
    }
}