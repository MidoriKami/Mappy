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
using Mappy.Utility;
using QuestLinkMarker = FFXIVClientStructs.FFXIV.Client.UI.Agent.QuestLinkMarker;

namespace Mappy.System.Modules;

public unsafe class Quest : ModuleBase {
    public override ModuleName ModuleName => ModuleName.QuestMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new QuestConfig();

    protected override void DrawMarkers(Viewport viewport, Map map) {
        var config = GetConfig<QuestConfig>();

        if (!config.HideUnacceptedQuests) DrawUnacceptedQuests(viewport, map);
        if (!config.HideAcceptedQuests) DrawAcceptedQuests(viewport, map);
        if (!config.HideLeveQuests) DrawLeveQuests(viewport, map);
    }

    private void DrawAcceptedQuests(Viewport viewport, Map map) {
        var mapData = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        var config = GetConfig<QuestConfig>();

        foreach (var quest in mapData->QuestDataSpan) {
            if (!quest.ShouldRender) continue;

            foreach (var questInfo in quest.MarkerData.Span) {
                if (LuminaCache<Level>.Instance.GetRow(questInfo.LevelId) is not { Map.Row: var levelMap, Territory.Row: var levelTerritory }) continue;
                if (levelMap != map.RowId && levelTerritory != map.TerritoryType.Row) continue;

                DrawUtilities.DrawMapIcon(new MappyMapIcon {
                    IconId = questInfo.IconId,
                    ObjectPosition = new Vector2(questInfo.X, questInfo.Z),

                    Tooltip = quest.Label.ToString(),

                    Radius = questInfo.Radius,
                    RadiusColor = config.InProgressColor,

                    VerticalPosition = questInfo.Y,
                }, config, viewport, map);
            }
        }

        var questLinkSpan = new ReadOnlySpan<QuestLinkMarker>(AgentMap.Instance()->MiniMapQuestLinkContainer.Markers, AgentMap.Instance()->MiniMapQuestLinkContainer.MarkerCount);
        foreach (var marker in questLinkSpan) {
            if (LuminaCache<Level>.Instance.GetRow(marker.LevelId) is not { X: var x, Y: var y, Z: var z }) continue;
            if (map.RowId != marker.SourceMapId) continue;

            DrawUtilities.DrawMapIcon(new MappyMapIcon {
                IconId = marker.IconId,
                ObjectPosition = new Vector2(x, z),

                GetTooltipFunc = () => marker.TooltipText.ToString(),

                RadiusColor = config.InProgressColor,

                VerticalPosition = y,
                
                OnClickAction = () => MappySystem.MapTextureController.LoadMap(marker.TargetMapId),
            }, config, viewport, map);
        }
    }
    
    private void DrawUnacceptedQuests(Viewport viewport, Map map) {
        var mapData = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        var config = GetConfig<QuestConfig>();
        
        foreach (var markerInfo in mapData->QuestMarkerData.GetAllMarkers()) {
            foreach (var markerData in markerInfo.MarkerData.Span) {
                if (LuminaCache<Level>.Instance.GetRow(markerData.LevelId) is not { Map.Row: var levelMap }) continue;
                if (levelMap != map.RowId) continue;

                DrawUtilities.DrawMapIcon(new MappyMapIcon {
                    IconId = markerData.IconId,
                    ObjectPosition = new Vector2(markerData.X, markerData.Z),

                    Tooltip = $"Lv. {markerData.RecommendedLevel} {markerData.TooltipString->ToString()}",

                    Radius = markerData.Radius,
                    RadiusColor = config.InProgressColor,
                    
                    VerticalPosition = markerData.Y,
                }, config, viewport, map);
            }
        }
    }
    
    private void DrawLeveQuests(Viewport viewport, Map map) {
        var mapData = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        var config = GetConfig<QuestConfig>();

        foreach (var quest in mapData->LevequestDataSpan) {
            if (!quest.ShouldRender) continue;
            
            foreach (var questInfo in quest.MarkerData.Span) {
                if (GetLevework(quest.ObjectiveId) is not { Flags: not 32 } ) continue;
                if (LuminaCache<Level>.Instance.GetRow(questInfo.LevelId) is not { Map.Row: var levelMap, Territory.Row: var levelTerritory } ) continue;
                if (levelMap != map.RowId && levelTerritory != map.TerritoryType.Row) continue;
                
                DrawUtilities.DrawMapIcon(new MappyMapIcon {
                    IconId = questInfo.IconId,
                    ObjectPosition = new Vector2(questInfo.X, questInfo.Z),

                    Tooltip = quest.Label.ToString(),

                    Radius = questInfo.Radius,
                    RadiusColor = config.LeveQuestColor,
                    
                    VerticalPosition = questInfo.Y,
                }, config, viewport, map);
            }
        }
        
        foreach (var markerInfo in mapData->ActiveLevequestMarkerData.Span) {
            if(LuminaCache<Level>.Instance.GetRow(markerInfo.LevelId) is not { Map.Row: var levelMap, Territory.Row: var levelTerritory } ) continue;
            if (levelMap != map.RowId && levelTerritory != map.TerritoryType.Row) continue;
            
            DrawUtilities.DrawMapIcon(new MappyMapIcon {
                IconId = markerInfo.IconId,
                TexturePosition = new Vector2(markerInfo.X, markerInfo.Z),

                Tooltip = markerInfo.TooltipString->ToString(),

                Radius = markerInfo.Radius,
                RadiusColor = config.LeveQuestColor,
                
                VerticalPosition = markerInfo.Y,
            }, config, viewport, map);
        }
    }

    private LeveWork? GetLevework(uint id) {
        foreach (var levework in QuestManager.Instance()->LeveQuestsSpan) {
            if (levework.LeveId == id) return levework;
        }

        return null;
    }
}