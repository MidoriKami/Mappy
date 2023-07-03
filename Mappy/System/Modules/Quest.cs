using System.Drawing;
using System.Numerics;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;

namespace Mappy.System.Modules;

[Category("ModuleColors")]
public interface IQuestColorConfig
{
    [ColorConfig("InProgressColor", 255, 69, 0, 45)]
    public Vector4 InProgressColor { get; set; }
    
    [ColorConfig("LeveQuestColor", 0, 133, 5, 97)]
    public Vector4 LeveQuestColor { get; set; }
}

[Category("ModuleConfig")]
public class QuestConfig : IModuleConfig, IIconConfig, ITooltipConfig, IQuestColorConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 11;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.White.AsVector4();
    public Vector4 InProgressColor { get; set; } = KnownColor.OrangeRed.AsVector4() with { W = 0.33f };
    public Vector4 LeveQuestColor { get; set; } = new Vector4(0, 133, 5, 97) / 255.0f;
    
    [BoolConfig("HideUnacceptedQuests")]
    public bool HideUnacceptedQuests { get; set; } = false;

    [BoolConfig("HideAcceptedQuests")]
    public bool HideAcceptedQuests { get; set; } = false;

    [BoolConfig("HideLeveQuests")]
    public bool HideLeveQuests { get; set; } = false;
}

public unsafe class Quest : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.QuestMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new QuestConfig();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!GetConfig<QuestConfig>().ShowIcon) return false;
        
        return base.ShouldDrawMarkers(map);
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<QuestConfig>();

        if (!config.HideUnacceptedQuests) DrawUnacceptedQuests(viewport, map);
        if (!config.HideAcceptedQuests) DrawAcceptedQuests(viewport, map);
        if (!config.HideLeveQuests) DrawLeveQuests(viewport, map);
    }
    
    private void DrawAcceptedQuests(Viewport viewport, Map map)
    {
        var mapData = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();

        foreach (var quest in mapData->QuestDataSpan)
        {
            foreach (var questInfo in quest.MarkerData.Span)
            {
                if (LuminaCache<Level>.Instance.GetRow(questInfo.LevelId) is not { Map.Row: var levelMap } levelData ) continue;
                if (levelMap != map.RowId) continue;
                
                DrawRegularObjective(questInfo.IconId, quest.Label.ToString(), levelData, viewport, map);
            }
        }
    }
    
    private void DrawUnacceptedQuests(Viewport viewport, Map map)
    {
        var mapData = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        
        foreach (var markerInfo in mapData->QuestMarkerData.GetAllMarkers())
        {
            foreach (var markerData in markerInfo.MarkerData.Span)
            {
                if (LuminaCache<Level>.Instance.GetRow(markerData.LevelId) is not { Map.Row: var levelMap} levelData) continue;
                if (levelMap != map.RowId) continue;
                // if (markerData.TooltipString->ToString().IsNullOrEmpty()) continue;

                DrawRegularObjective(markerData.IconId, $"Lv. {markerData.RecommendedLevel} {markerData.TooltipString->ToString()}", levelData, viewport, map);
            }
        }
    }
    
    private void DrawLeveQuests(Viewport viewport, Map map)
    {
        var mapData = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();

        foreach (var quest in mapData->LevequestDataSpan)
        {
            foreach (var questInfo in quest.MarkerData.Span)
            {
                if (FindLevework(quest.ObjectiveId) is not { Flags: not 32 } ) continue;
                if (LuminaCache<Level>.Instance.GetRow(questInfo.LevelId) is not { Map.Row: var levelMap } levelData ) continue;
                if (levelMap != map.RowId) continue;
                
                DrawLeveObjective(questInfo.IconId, quest.Label.ToString(), levelData, viewport, map);
            }
        }
        
        foreach (var markerInfo in mapData->ActiveLevequestMarkerData.Span)
        {
            if(LuminaCache<Level>.Instance.GetRow(markerInfo.LevelId) is not { Map.Row: var levelMap } levelData ) continue;
            if(levelMap != map.RowId) continue;
            
            DrawLeveObjective(markerInfo.IconId, markerInfo.TooltipString->ToString(), levelData, viewport, map);
        }
    }

    private LeveWork? FindLevework(uint id)
    {
        foreach (var levework in QuestManager.Instance()->LeveQuestsSpan)
        {
            if (levework.LeveId == id) return levework;
        }

        return null;
    }

    private void DrawRegularObjective(uint icon, string tooltip, Level levelData, Viewport viewport, Map map)
    {
        var ringColor = GetConfig<QuestConfig>().InProgressColor;
        var tooltipColor =  GetConfig<QuestConfig>().TooltipColor;
        var scale = GetConfig<QuestConfig>().IconScale;
        var showTooltip = GetConfig<QuestConfig>().ShowTooltip;
        
        DrawUtilities.DrawLevelObjective(levelData, icon, tooltip, ringColor, tooltipColor, viewport, map, showTooltip, scale);
    }
    
    private void DrawLeveObjective(uint icon, string tooltip, Level levelData, Viewport viewport, Map map)
    {
        var ringColor = GetConfig<QuestConfig>().LeveQuestColor;
        var tooltipColor =  GetConfig<QuestConfig>().TooltipColor;
        var scale = GetConfig<QuestConfig>().IconScale;
        var showTooltip = GetConfig<QuestConfig>().ShowTooltip;
        
        DrawUtilities.DrawLevelObjective(levelData, icon, tooltip, ringColor, tooltipColor, viewport, map, showTooltip, scale, 50.0f);
    }
}