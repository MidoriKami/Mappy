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

public class MiscMarkers : ModuleBase
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
        foreach (var marker in MappySystem.QuestController.CustomTalkMarkers)
        {
            DrawCustomTalkMarker(marker, map);
        }
    }
    
    private void DrawTripleTriadMarkers(Map map)
    {
        foreach (var marker in MappySystem.QuestController.TripleTriadMarkers)
        {
            DrawTripleTriadMarker(marker, map);
        }
    }
    
    private void DrawGuildleveAssignmentMarkers(Map map)
    {
        foreach (var marker in MappySystem.QuestController.GuildleveAssignmentMarkers)
        {
            DrawGuildleveAssignmentMarker(marker, map);
        }
    }

    private void DrawMiscMarkers(Map map)
    {
        foreach (var marker in MappySystem.QuestController.MiscNetworkMarkers)
        {
            DrawMiscMarker(marker, map);
        }
    }

    private void DrawCustomTalkMarker(NetworkMarkerInfo markerInfo, Map map)
    {
        if (markerInfo.GetLuminaData<CustomTalk>() is not { } customTalkData) return;
        if (customTalkData is not { MainOption.RawString: var mainOption, SubOption.RawString: var subOption }) return;

        var tooltip = mainOption.IsNullOrEmpty() ? subOption : mainOption;

        DrawObjective(markerInfo, map, tooltip);
    }

    private void DrawTripleTriadMarker(NetworkMarkerInfo markerInfo, Map map)
    {
        if (markerInfo.GetLuminaData<TripleTriad>() is not { } triadInfo ) return;
        if (LuminaCache<Addon>.Instance.GetRow(9224) is not { Text.RawString: var triadMatch }) return;
        
        var cardRewards = triadInfo.ItemPossibleReward
            .Where(reward => reward.Row is not 0)
            .Select(reward => reward.Value)
            .OfType<Item>()
            .Select(item => item.Name.RawString);
        
        DrawObjective(markerInfo, map, triadMatch, string.Join("\n", cardRewards));
    }
    
    private void DrawGuildleveAssignmentMarker(NetworkMarkerInfo markerInfo, Map map)
    {
        if (markerInfo.GetLuminaData<GuildleveAssignment>() is not { Type.RawString: var markerTooltip }) return;
        
        DrawObjective(markerInfo, map, markerTooltip);
    }
    
    private void DrawMiscMarker(NetworkMarkerInfo marker, Map map)
    {
        switch (marker.ObjectiveId)
        {
            case 0x170001:
                DrawObjective(marker, map, "Guildhests");
                break;
        }
    }

    private void DrawObjective(NetworkMarkerInfo markerInfo, Map map, string tooltip, string? secondaryTooltip = null)
    {
        if (markerInfo.GetLevelData() is not { } levelData) return;
        if (levelData.Map.Row != map.RowId) return;
        
        var config = GetConfig<MiscConfig>();
        var position = Position.GetObjectPosition(new Vector2(levelData.X, levelData.Z), map);
        var scale = markerInfo.Flags is 1 ? config.IconScale / 2.0f : config.IconScale;
        
        DrawUtilities.DrawIcon(markerInfo.MapIcon, position, scale);

        if (secondaryTooltip is null && config.ShowTooltip && !tooltip.IsNullOrEmpty()) DrawUtilities.DrawTooltip(tooltip, config.TooltipColor, markerInfo.MapIcon);
        if (secondaryTooltip is not null && config.ShowTooltip) DrawUtilities.DrawMultiTooltip(tooltip, secondaryTooltip, config.TooltipColor, markerInfo.MapIcon);
    }
}