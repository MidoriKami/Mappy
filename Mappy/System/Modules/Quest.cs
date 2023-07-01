using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Utilities;
using KamiLib.Windows;
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

    public override void LoadForMap(MapData mapData) { }

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
        foreach (var quest in QuestManager.Instance()->NormalQuestsSpan)
        {
            if (quest is { QuestId: 0 }) continue;

            foreach (var level in QuestHelpers.GetActiveLevelsForQuest(quest, map.RowId).Where(levelRow => levelRow.RowId is not 0))
            {
                if (LuminaCache<CustomQuestSheet>.Instance.GetRow(quest.QuestId + 65536u) is not { JournalGenre.Value.Icon: var journalIcon, Name.RawString: var questName }) continue;
                
                DrawRegularObjective((uint) journalIcon, questName, level, viewport, map);
            }
        }
    }
    
    private void DrawUnacceptedQuests(Viewport viewport, Map map)
    {
        foreach (var markerInfo in MappySystem.QuestController.QuestMarkers)
        {
            if (markerInfo is {Flags: 6}) continue;
            if (markerInfo.GetLevelData() is not { Map.Row: var levelMap } levelData) continue;
            if (levelMap != map.RowId) continue;
            if (LuminaCache<CustomQuestSheet>.Instance.GetRow(markerInfo.ObjectiveId) is not { ClassJobLevel0: var questLevel, Name.RawString: var questName } ) continue;
            
            DrawRegularObjective(markerInfo.MapIcon, $"Lv. {questLevel} {questName}", levelData, viewport, map);
        }
    }
    
    private void DrawLeveQuests(Viewport viewport, Map map)
    {
        var anyActive = false;
        
        foreach (var quest in QuestManager.Instance()->LeveQuestsSpan)
        {
            if(quest is { LeveId: 0 } or { Flags:44 } ) continue; // 44 = LeveComplete
            if (LuminaCache<Leve>.Instance.GetRow(quest.LeveId) is not { LevelStart.Row: var leveLevelStart, JournalGenre.Row: var leveJournalGenre, Name.RawString: var leveQuestName }) continue;
            if (LuminaCache<Level>.Instance.GetRow(leveLevelStart) is not { Map.Row: var levelMap } levelData ) continue;
            if (levelMap != map.RowId) continue;
            if (LuminaCache<JournalGenre>.Instance.GetRow(leveJournalGenre) is not { Icon: var genreIcon }) continue;
            
            DrawLeveObjective((uint) genreIcon, leveQuestName, levelData, viewport, map);

            if(quest is not { Flags: 32 }) continue; // In Progress
            
            anyActive = true;
            foreach (var activeLevel in MappySystem.QuestController.ActiveLevequestLevels)
            {
                if (LuminaCache<Level>.Instance.GetRow(activeLevel) is not { } activeLevelData) continue;
                
                DrawLeveObjective((uint)genreIcon, leveQuestName, activeLevelData, viewport, map);
            }
        }

        if (!anyActive && MappySystem.QuestController.ActiveLevequestLevels.Count > 0)
        {
            MappySystem.QuestController.ActiveLevequestLevels.Clear();
        }
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