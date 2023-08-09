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

[Category("ModuleColors", 2)]
public interface IQuestColorConfig
{
    [ColorConfig("InProgressColor", 255, 69, 0, 45)]
    public Vector4 InProgressColor { get; set; }
    
    [ColorConfig("LeveQuestColor", 0, 133, 5, 97)]
    public Vector4 LeveQuestColor { get; set; }
}

[Category("DirectionalMarker", 1)]
public interface IQuestDistanceMarkerConfig
{
    [BoolConfig("DirectionalMarker")]
    public bool EnableDirectionalMarker { get; set; }
    
    [FloatConfig("DistanceThreshold", 0.0f, 50.0f)]
    public float DistanceThreshold { get; set; }
}

[Category("ModuleConfig")]
public class QuestConfig : IModuleConfig, IIconConfig, ITooltipConfig, IQuestColorConfig, IQuestDistanceMarkerConfig
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

    public bool EnableDirectionalMarker { get; set; } = true;
    public float DistanceThreshold { get; set; } = 20.0f;
}

public unsafe class Quest : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.QuestMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new QuestConfig();
    
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
        var config = GetConfig<QuestConfig>();

        foreach (var quest in mapData->QuestDataSpan)
        {
            if (quest.ShouldRender != 1) continue;
            
            foreach (var questInfo in quest.MarkerData.Span)
            {
                if (LuminaCache<Level>.Instance.GetRow(questInfo.LevelId) is not { Map.Row: var levelMap }) continue;
                if (levelMap != map.RowId) continue;
                
                DrawUtilities.DrawMapIcon(new MappyMapIcon
                {
                    IconId = questInfo.IconId,
                    ObjectPosition = new Vector2(questInfo.X, questInfo.Z),
                    IconScale = config.IconScale,
                    ShowIcon = config.ShowIcon,

                    Tooltip = quest.Label.ToString(),
                    TooltipColor = config.TooltipColor,
                    ShowTooltip = config.ShowTooltip,

                    Radius = questInfo.Radius,
                    RadiusColor = config.InProgressColor,
                    
                    ShowDirectionalIndicator = config.EnableDirectionalMarker,
                    VerticalPosition = questInfo.Y,
                    VerticalThreshold = config.DistanceThreshold,
                }, viewport, map);
            }
        }
    }
    
    private void DrawUnacceptedQuests(Viewport viewport, Map map)
    {
        var mapData = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        var config = GetConfig<QuestConfig>();
        
        foreach (var markerInfo in mapData->QuestMarkerData.GetAllMarkers())
        {
            foreach (var markerData in markerInfo.MarkerData.Span)
            {
                if (LuminaCache<Level>.Instance.GetRow(markerData.LevelId) is not { Map.Row: var levelMap}) continue;
                if (levelMap != map.RowId) continue;

                DrawUtilities.DrawMapIcon(new MappyMapIcon
                {
                    IconId = markerData.IconId,
                    ObjectPosition = new Vector2(markerData.X, markerData.Z),
                    IconScale = config.IconScale,
                    ShowIcon = config.ShowIcon,

                    Tooltip = $"Lv. {markerData.RecommendedLevel} {markerData.TooltipString->ToString()}",
                    TooltipColor = config.TooltipColor,
                    ShowTooltip = config.ShowTooltip,

                    Radius = markerData.Radius,
                    RadiusColor = config.InProgressColor,
                    
                    ShowDirectionalIndicator = config.EnableDirectionalMarker,
                    VerticalPosition = markerData.Y,
                    VerticalThreshold = config.DistanceThreshold,
                }, viewport, map);
            }
        }
    }
    
    private void DrawLeveQuests(Viewport viewport, Map map)
    {
        var mapData = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        var config = GetConfig<QuestConfig>();

        foreach (var quest in mapData->LevequestDataSpan)
        {
            if (quest.ShouldRender != 1) continue;
            
            foreach (var questInfo in quest.MarkerData.Span)
            {
                if (GetLevework(quest.ObjectiveId) is not { Flags: not 32 } ) continue;
                if (LuminaCache<Level>.Instance.GetRow(questInfo.LevelId) is not { Map.Row: var levelMap } ) continue;
                if (levelMap != map.RowId) continue;
                
                DrawUtilities.DrawMapIcon(new MappyMapIcon
                {
                    IconId = questInfo.IconId,
                    ObjectPosition = new Vector2(questInfo.X, questInfo.Z),
                    IconScale = config.IconScale,
                    ShowIcon = config.ShowIcon,

                    Tooltip = quest.Label.ToString(),
                    TooltipColor = config.TooltipColor,
                    ShowTooltip = config.ShowTooltip,

                    Radius = questInfo.Radius,
                    RadiusColor = config.LeveQuestColor,
                    
                    ShowDirectionalIndicator = config.EnableDirectionalMarker,
                    VerticalPosition = questInfo.Y,
                    VerticalThreshold = config.DistanceThreshold,
                }, viewport, map);
            }
        }
        
        foreach (var markerInfo in mapData->ActiveLevequestMarkerData.Span)
        {
            if(LuminaCache<Level>.Instance.GetRow(markerInfo.LevelId) is not { Map.Row: var levelMap } ) continue;
            if(levelMap != map.RowId) continue;
            
            DrawUtilities.DrawMapIcon(new MappyMapIcon
            {
                IconId = markerInfo.IconId,
                TexturePosition = new Vector2(markerInfo.X, markerInfo.Z),
                IconScale = config.IconScale,
                Tooltip = markerInfo.TooltipString->ToString(),
                TooltipColor = config.TooltipColor,
                Radius = markerInfo.Radius,
                RadiusColor = config.LeveQuestColor,
                ShowIcon = config.ShowIcon,
                ShowTooltip = config.ShowTooltip,
                ShowDirectionalIndicator = config.EnableDirectionalMarker,
                VerticalPosition = markerInfo.Y,
                VerticalThreshold = config.DistanceThreshold,
            }, viewport, map);
        }
    }

    private LeveWork? GetLevework(uint id)
    {
        foreach (var levework in QuestManager.Instance()->LeveQuestsSpan)
        {
            if (levework.LeveId == id) return levework;
        }

        return null;
    }
}