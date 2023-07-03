using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Utility;
using KamiLib.Atk;
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

[Category("ModuleConfig")]
public class MiscConfig : IModuleConfig, IIconConfig, ITooltipConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 11;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.White.AsVector4();
}

public unsafe class MiscMarkers : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.MiscMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new MiscConfig();
    
    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!GetConfig<MiscConfig>().ShowIcon) return false;
        
        return base.ShouldDrawMarkers(map);
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        DrawCustomTalkMarkers(map);
        DrawTripleTriadMarkers(map);
        DrawGuildleveAssignmentMarkers(map);
        DrawMiscMarkers(map);
        DrawBicolorGemstoneMarkers(map);
        DrawTestCode();
    }

    private void DrawTestCode()
    {
        var data = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();

        DebugWindow.Print("QuestDataSpan");
        foreach (var marker in data->QuestDataSpan)
        {
            PrintMarker(marker);
        }
        
        DebugWindow.Print("LevequestDataSpan");
        foreach (var marker in data->LevequestDataSpan)
        {
            PrintMarker(marker);
        }
        
        DebugWindow.Print("ActiveLevequestMarkerData");
        foreach (var marker in data->ActiveLevequestMarkerData.Span)
        {
            PrintMarker(marker);
        }
        
        DebugWindow.Print("QuestMapMarkerData");
        foreach (var markerInfo in data->QuestMarkerData.GetAllMarkers())
        {
            PrintMarker(markerInfo);

            foreach (var marker in markerInfo.MarkerData.Span)
            {
                PrintMarker(marker);
            }
        }
        
        DebugWindow.Print("QuestMarkerData");
        foreach (var marker in data->SimpleQuestMarkerData.GetEnumerable())
        {
            PrintMarker(marker);
        }
        
        DebugWindow.Print("GuildLeveAssignmentMapMarkerData");
        foreach (var tooltipMapMarker in data->GuildLeveAssignmentMapMarkerData.GetAllMarkers())
        {
            PrintMarker(tooltipMapMarker);

            foreach (var marker in tooltipMapMarker.MarkerData.Span)
            {
                PrintMarker(marker);
            }
        }
        
        DebugWindow.Print("GuildOrderGuideMarkerData");
        foreach (var tooltipMapMarker in data->GuildOrderGuideMarkerData.GetAllMarkers())
        {
            PrintMarker(tooltipMapMarker);

            foreach (var marker in tooltipMapMarker.MarkerData.Span)
            {
                PrintMarker(marker);
            }
        }
        
        DebugWindow.Print("TripleTriadMarkerData");
        foreach (var tooltipMapMarker in data->TripleTriadMarkerData.GetAllMarkers())
        {
            PrintMarker(tooltipMapMarker);

            foreach (var marker in tooltipMapMarker.MarkerData.Span)
            {
                PrintMarker(marker);
            }
        }
        
        DebugWindow.Print("CustomTalkMarkerData");
        foreach (var tooltipMapMarker in data->CustomTalkMarkerData.GetAllMarkers())
        {
            PrintMarker(tooltipMapMarker);

            foreach (var marker in tooltipMapMarker.MarkerData.Span)
            {
                PrintMarker(marker);
            }
        }
        
        DebugWindow.Print("SimpleCustomTalkMarkerData");
        foreach (var marker in data->SimpleCustomTalkMarkerData.GetEnumerable())
        {
            PrintMarker(marker);
        }
        
        DebugWindow.Print("BicolorGemstoneVendorMarkerData");
        foreach (var tooltipMapMarker in data->GemstoneTraderMarkerData.GetAllMarkers())
        {
            PrintMarker(tooltipMapMarker);

            foreach (var marker in tooltipMapMarker.MarkerData.Span)
            {
                PrintMarker(marker);
            }
        }
        
        DebugWindow.Print("SimpleGemstoneTraderMarkerData");
        foreach (var marker in data->SimpleGemstoneTraderMarkerData.GetEnumerable())
        {
            PrintMarker(marker);
        }
    }

    private void PrintMarker(MarkerInfo info)
    {
        DebugWindow.Print($"[MarkerInfo] {info.ObjectiveId, 7} {info.ShouldRender, 7} {info.RecommendedLevel, 7} {info.Label.ToString()}");
    }

    private void PrintMarker(MapMarkerData data)
    {
        DebugWindow.Print($"\t\t -> [MapMarkerData] {data.IconId, 7} {data.ObjectiveId, 7} {data.LevelId, 7} {data.RecommendedLevel, 7} {data.TooltipString->ToString()}");
    }

    private void PrintMarker(SimpleMapMarkerData data)
    {
        DebugWindow.Print($"[SimpleMapMarkerData] {data.IconId, 7} {data.LevelId, 7} {data.ObjectiveId, 7} {data.Flags, 7}");
    }

    private void DrawCustomTalkMarkers(Map map)
    {
        // var data = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        //
        // foreach (var markerData in data->CustomTalkMarkerData.DataSpan)
        // {
        //     var tooltip = GetCustomTalkString(markerData.Value->ObjectiveId);
        //
        //     DrawObjective(markerData, map, tooltip);
        // }
    }
    
    private void DrawTripleTriadMarkers(Map map)
    {
        // var data = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        //
        // foreach (var markerData in data->TripleTriadMarkerData.DataSpan)
        // {
        //     if (markerData.Value is null) continue;
        //     
        //     if (LuminaCache<TripleTriad>.Instance.GetRow(markerData.Value->ObjectiveId) is not { } triadInfo ) return;
        //     if (LuminaCache<Addon>.Instance.GetRow(9224) is not { Text.RawString: var triadMatch }) return;
        //
        //     var cardRewards = triadInfo.ItemPossibleReward
        //         .Where(reward => reward.Row is not 0)
        //         .Select(reward => reward.Value)
        //         .OfType<Item>()
        //         .Select(item => item.Name.RawString);
        //
        //     DrawObjective(markerData, map, triadMatch, string.Join("\n", cardRewards));
        // }
    }
    
    private void DrawGuildleveAssignmentMarkers(Map map)
    {
        var data = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();

        // foreach (var markerData in data->GuildLeveAssignmentMarkerData.DataSpan)
        // {
        //     if (LuminaCache<GuildleveAssignment>.Instance.GetRow(markerData.Value->ObjectiveId) is not { Type.RawString: var markerTooltip }) return;
        //
        //     DrawObjective(markerData, map, markerTooltip);
        // }
    }

    private void DrawMiscMarkers(Map map)
    {
        // var data = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        //
        // foreach (var markerData in data->GuildOrderGuideMarkerData.DataSpan)
        // {
        //     if (markerData.Value is null) continue;
        //
        //     DrawObjective(markerData, map, markerData.Value->Tooltip.ToString());
        // }
    }
    
    private void DrawBicolorGemstoneMarkers(Map map)
    {
        // var data = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        //
        // foreach (var markerData in data->BicolorGemstoneVendorMarkerData.DataSpan)
        // {
        //     var tooltip = GetCustomTalkString(markerData.Value->ObjectiveId);
        //     
        //     DrawObjective(markerData, map, tooltip);
        // }
    }

    private string GetCustomTalkString(uint rowId)
    {
        if (LuminaCache<CustomTalk>.Instance.GetRow(rowId) is not { } customTalkData) return string.Empty;
        if (customTalkData is not { MainOption.RawString: var mainOption, SubOption.RawString: var subOption }) return string.Empty;

        return mainOption.IsNullOrEmpty() ? subOption : mainOption;
    }
    
    // private void DrawObjective(NonstandardMarker* specialMarker, Map map, string tooltip, string? secondaryTooltip = null)
    // {
    //     if (LuminaCache<Level>.Instance.GetRow(specialMarker->MarkerData->LevelId) is not { } levelData) return;
    //     if (levelData.Map.Row != map.RowId) return;
    //     
    //     DrawObjective(levelData, map, tooltip, specialMarker->MarkerData->IconId, secondaryTooltip);
    // }
    //
    private void DrawObjective(SimpleMapMarkerData* markerInfo, Map map, string tooltip, string? secondaryTooltip = null)
    {
        if (LuminaCache<Level>.Instance.GetRow(markerInfo->LevelId) is not { } levelData) return;
        if (levelData.Map.Row != map.RowId) return;
        
        DrawObjective(levelData, map, tooltip, markerInfo->IconId, secondaryTooltip);
    }

    private void DrawObjective(Level levelData, Map map, string tooltip, uint iconId, string? secondaryTooltip = null)
    {
        var config = GetConfig<MiscConfig>();
        var position = Position.GetObjectPosition(new Vector2(levelData.X, levelData.Z), map);
        
        DrawUtilities.DrawIcon(iconId, position);

        if (secondaryTooltip is null && config.ShowTooltip && !tooltip.IsNullOrEmpty()) DrawUtilities.DrawTooltip(tooltip, config.TooltipColor, iconId);
        if (secondaryTooltip is not null && config.ShowTooltip) DrawUtilities.DrawMultiTooltip(tooltip, secondaryTooltip, config.TooltipColor, iconId);
    }
}