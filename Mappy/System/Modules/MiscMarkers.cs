using System.Collections.Generic;
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
    private Dictionary<uint, string> CardRewardCache = new();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!GetConfig<MiscConfig>().ShowIcon) return false;
        
        return base.ShouldDrawMarkers(map);
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var data = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        
        DrawMapMarkerContainer(data->CustomTalkMarkerData, map);
        DrawMapMarkerContainer(data->GuildLeveAssignmentMapMarkerData, map);
        DrawMapMarkerContainer(data->GuildOrderGuideMarkerData, map);
        DrawMapMarkerContainer(data->GemstoneTraderMarkerData, map);
        
        DrawTripleTriadMarkers(map);
    }
    
    private void DrawTripleTriadMarkers(Map map)
    {
        var data = (ClientStructsMapData*) FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();

        foreach (var markerInfo in data->TripleTriadMarkerData.GetAllMarkers())
        {
            foreach (var subLocation in markerInfo.MarkerData.Span)
            {
                if (!CardRewardCache.ContainsKey(subLocation.ObjectiveId))
                {
                    if (LuminaCache<TripleTriad>.Instance.GetRow(subLocation.ObjectiveId) is not { } triadInfo) continue;
                    
                    var cardRewards = triadInfo.ItemPossibleReward
                        .Where(reward => reward.Row is not 0)
                        .Select(reward => reward.Value)
                        .OfType<Item>()
                        .Select(item => item.Name.RawString);
                    
                    CardRewardCache.Add(subLocation.ObjectiveId, string.Join("\n", cardRewards));
                }
                
                DrawObjective(subLocation, map, subLocation.TooltipString->ToString(), CardRewardCache[subLocation.ObjectiveId]);
            }
        }
    }
    
    private void DrawMapMarkerContainer(MapMarkerContainer container, Map map)
    {
        foreach (var markerInfo in container.GetAllMarkers())
        {
            foreach (var subLocation in markerInfo.MarkerData.Span)
            {
                DrawObjective(subLocation, map, subLocation.TooltipString->ToString());
            }
        }
    }
    
    private void DrawObjective(MapMarkerData markerInfo, Map map, string tooltip, string? secondaryTooltip = null)
    {
        if (LuminaCache<Level>.Instance.GetRow(markerInfo.LevelId) is not { } levelData) return;
        if (levelData.Map.Row != map.RowId) return;
        
        DrawObjective(levelData, map, tooltip, markerInfo.IconId, secondaryTooltip);
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