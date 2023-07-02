using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Utility;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Utilities;
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
    }

    private void DrawCustomTalkMarkers(Map map)
    {
        var data = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();

        foreach (var markerData in data->CustomTalkMarkerData.DataSpan)
        {
            if (LuminaCache<CustomTalk>.Instance.GetRow(markerData.Value->ObjectiveId) is not { } customTalkData) return;
            if (customTalkData is not { MainOption.RawString: var mainOption, SubOption.RawString: var subOption }) return;

            var tooltip = mainOption.IsNullOrEmpty() ? subOption : mainOption;

            DrawObjective(markerData, map, tooltip);
        }
    }
    
    private void DrawTripleTriadMarkers(Map map)
    {
        var data = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();

        foreach (var markerData in data->TripleTriadMarkerData.DataSpan)
        {
            if (markerData.Value is null) continue;
            
            if (LuminaCache<TripleTriad>.Instance.GetRow(markerData.Value->ObjectiveId) is not { } triadInfo ) return;
            if (LuminaCache<Addon>.Instance.GetRow(9224) is not { Text.RawString: var triadMatch }) return;
        
            var cardRewards = triadInfo.ItemPossibleReward
                .Where(reward => reward.Row is not 0)
                .Select(reward => reward.Value)
                .OfType<Item>()
                .Select(item => item.Name.RawString);
        
            DrawObjective(markerData, map, triadMatch, string.Join("\n", cardRewards));
        }
    }
    
    private void DrawGuildleveAssignmentMarkers(Map map)
    {
        var data = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();

        foreach (var markerData in data->GuildLeveAssignmentMarkerData.DataSpan)
        {
            if (LuminaCache<GuildleveAssignment>.Instance.GetRow(markerData.Value->ObjectiveId) is not { Type.RawString: var markerTooltip }) return;
        
            DrawObjective(markerData, map, markerTooltip);
        }
    }

    private void DrawMiscMarkers(Map map)
    {
        var data = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();

        foreach (var markerData in data->GuildOrderGuideMarkerData.DataSpan)
        {
            if (markerData.Value is null) continue;

            DrawObjective(markerData, map, markerData.Value->Tooltip.ToString());
        }
    }
    
    private void DrawObjective(NonstandardMarker* specialMarker, Map map, string tooltip, string? secondaryTooltip = null)
    {
        if (LuminaCache<Level>.Instance.GetRow(specialMarker->MarkerData->LevelId) is not { } levelData) return;
        if (levelData.Map.Row != map.RowId) return;
        
        DrawObjective(levelData, map, tooltip, specialMarker->MarkerData->IconId, 0, secondaryTooltip);
    }
    
    private void DrawObjective(StandardMapMarkerData* markerInfo, Map map, string tooltip, string? secondaryTooltip = null)
    {
        if (LuminaCache<Level>.Instance.GetRow(markerInfo->LevelId) is not { } levelData) return;
        if (levelData.Map.Row != map.RowId) return;
        
        DrawObjective(levelData, map, tooltip, markerInfo->IconId, markerInfo->Flags, secondaryTooltip);
    }

    private void DrawObjective(Level levelData, Map map, string tooltip, uint iconId, int flags, string? secondaryTooltip = null)
    {
        var config = GetConfig<MiscConfig>();
        var position = Position.GetObjectPosition(new Vector2(levelData.X, levelData.Z), map);
        
        DrawUtilities.DrawIcon(iconId, position);

        if (secondaryTooltip is null && config.ShowTooltip && !tooltip.IsNullOrEmpty()) DrawUtilities.DrawTooltip(tooltip, config.TooltipColor, iconId);
        if (secondaryTooltip is not null && config.ShowTooltip) DrawUtilities.DrawMultiTooltip(tooltip, secondaryTooltip, config.TooltipColor, iconId);
    }
}