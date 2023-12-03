using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;
using Mappy.Utility;
using Map = Lumina.Excel.GeneratedSheets.Map;
using QuestLinkMarker = FFXIVClientStructs.FFXIV.Client.UI.Agent.QuestLinkMarker;

namespace Mappy.System.Modules;

public unsafe class Quest : ModuleBase {
    public override ModuleName ModuleName => ModuleName.QuestMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new QuestConfig();

    protected override void UpdateMarkers(Viewport viewport, Map map) {
        var mapData = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        var config = GetConfig<QuestConfig>();

        if (!config.HideUnacceptedQuests) {
            mapData->UnacceptedQuests.DrawMarkers(DrawMarker, map, null, null, () => config.UnacceptedQuestColor);
        }
        
        if (!config.HideAcceptedQuests) {
            mapData->QuestDataSpan.DrawMarkers(DrawMarker, map, null, null, null, () => config.InProgressColor, config.IgnoreJournalSetting);
            DrawQuestLinkMarkers(map);
        }

        if (!config.HideLeveQuests) {
            mapData->LevequestDataSpan.DrawMarkers(DrawMarker, map, null, null, LevequestFilter, () => config.LeveQuestColor);
            mapData->ActiveLevequest.DrawMarkers(DrawMarker, map, null, null, () => config.LeveQuestColor);
        }
    }
    
    private void DrawMarker(MapMarkerData marker, Map map, Func<MapMarkerData, string>? extraTooltip = null, Func<object>? getExtraData = null) {
        if (marker.MapId != map.RowId && marker.TerritoryTypeId != map.TerritoryType.Row) return;

        UpdateIcon((marker.ObjectiveId, marker.LevelId), () => new MappyMapIcon {
            MarkerId = (marker.ObjectiveId, marker.LevelId),
            IconId = marker.IconId,
            ObjectPosition = new Vector2(marker.X, marker.Z),
            Tooltip = marker.TooltipString->ToString(),
            MinimumRadius = marker.Radius,
            RadiusColor = (Vector4)getExtraData!.Invoke(),
            VerticalPosition = marker.Y,
        }, icon => {
            icon.RadiusColor = (Vector4)getExtraData!.Invoke();
            icon.IconId = marker.IconId;
        });
    }

    private void DrawQuestLinkMarkers(Map map) {
        var config = GetConfig<QuestConfig>();

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
                icon.IconId = marker.IconId;
            });
        }
    }

    private bool LevequestFilter(MarkerInfo markerInfo) {
        foreach (var levework in QuestManager.Instance()->LeveQuestsSpan) {
            if (levework.LeveId == markerInfo.ObjectiveId) {
                if (levework is not { Flags: not 32 }) return true;
            }
        }
        
        return false;
    }
}