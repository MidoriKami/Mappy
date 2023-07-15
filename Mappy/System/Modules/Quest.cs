using System.Drawing;
using System.Numerics;
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

        // foreach (var quest in mapData->QuestDataSpan)
        // {
        //     foreach (var questInfo in quest.MarkerData.Span)
        //     {
        //         if (LuminaCache<Level>.Instance.GetRow(questInfo.LevelId) is not { Map.Row: var levelMap }) continue;
        //         if (levelMap != map.RowId) continue;
        //         
        //         DrawRegularObjective(questInfo, quest.Label.ToString(), viewport, map);
        //     }
        // }

        foreach (var quest in mapData->QuestMarkerData.GetAllMarkers())
        {
            foreach (var marker in quest.MarkerData.Span)
            {
                if (LuminaCache<Level>.Instance.GetRow(marker.LevelId) is not { Map.Row: var levelMap }) continue;
                if (levelMap != map.RowId) continue;
                
                DrawRegularObjective(marker, quest.Label.ToString(), viewport, map);
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
                if (LuminaCache<Level>.Instance.GetRow(markerData.LevelId) is not { Map.Row: var levelMap}) continue;
                if (levelMap != map.RowId) continue;

                DrawRegularObjective(markerData, $"Lv. {markerData.RecommendedLevel} {markerData.TooltipString->ToString()}", viewport, map);
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
                if (LuminaCache<Level>.Instance.GetRow(questInfo.LevelId) is not { Map.Row: var levelMap } ) continue;
                if (levelMap != map.RowId) continue;
                
                DrawLeveObjective(questInfo, quest.Label.ToString(), viewport, map);
            }
        }
        
        foreach (var markerInfo in mapData->ActiveLevequestMarkerData.Span)
        {
            if(LuminaCache<Level>.Instance.GetRow(markerInfo.LevelId) is not { Map.Row: var levelMap } ) continue;
            if(levelMap != map.RowId) continue;
            
            DrawLeveObjective(markerInfo, markerInfo.TooltipString->ToString(), viewport, map);
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
    
    private void DrawRegularObjective(MapMarkerData marker, string tooltip, Viewport viewport, Map map)
        => DrawObjective(marker, tooltip, viewport, map, GetConfig<QuestConfig>().InProgressColor);
    
    private void DrawLeveObjective(MapMarkerData marker, string tooltip, Viewport viewport, Map map)
        => DrawObjective(marker, tooltip, viewport, map, GetConfig<QuestConfig>().LeveQuestColor);

    private void DrawObjective(MapMarkerData marker, string tooltip, Viewport viewport, Map map, Vector4 color)
    {
        var config = GetConfig<QuestConfig>();
        
        DrawUtilities.DrawLevelIcon(new Vector2(marker.X, marker.Z), marker.Radius, viewport, map, marker.IconId, color, config.IconScale, 0.0f);

        if(config.ShowTooltip && marker.Radius < 5.0f) DrawUtilities.DrawTooltip(marker.IconId, config.TooltipColor, tooltip);
        if(config.ShowTooltip && marker.Radius >= 5.0f) DrawUtilities.DrawLevelTooltip(new Vector2(marker.X, marker.Z), marker.Radius, viewport, map, marker.IconId, config.TooltipColor, tooltip);
    }
}