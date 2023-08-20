using System.Drawing;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using ModuleBase = Mappy.Abstracts.ModuleBase;

namespace Mappy.System.Modules;

[Category("ModuleColors")]
public class TemporaryMarkersConfig : IModuleConfig, IIconConfig, ITooltipConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 13;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.LightSkyBlue.AsVector4();
    
    [ColorConfig("CircleColor", 65, 105, 225, 45)]
    public Vector4 CircleColor { get; set; } = KnownColor.RoyalBlue.AsVector4() with { W = 0.33f };
}

public unsafe class TemporaryMarkers : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.TemporaryMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new TemporaryMarkersConfig();
    
    public static TemporaryMapMarker? FlagMarker { get; private set; }
    public static TemporaryMapMarker? GatheringMarker { get; private set; }

    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<TemporaryMarkersConfig>();

        if (GatheringMarker is not null && GatheringMarker.MapID == map.RowId)
        {
            DrawUtilities.DrawMapIcon(new MappyMapIcon
            {
                IconId = GatheringMarker.IconID,
                ObjectPosition = GatheringMarker.Position,
                
                Tooltip = GatheringMarker.TooltipText,
                
                Radius = GatheringMarker.Radius,
                RadiusColor = config.CircleColor,
            }, config, viewport, map);
            
            GatheringMarker.ShowContextMenu(viewport, map);
        }
   
        if (FlagMarker is not null && FlagMarker.MapID == map.RowId && AgentMap.Instance() is not null && AgentMap.Instance()->IsFlagMarkerSet is 1)
        {
            DrawUtilities.DrawMapIcon(new MappyMapIcon
            {
                IconId = FlagMarker.IconID,
                ObjectPosition = FlagMarker.Position,
                
                Tooltip = FlagMarker.TooltipText,
            }, config, viewport, map);

            FlagMarker.ShowContextMenu(viewport, map);
        }
    }
    
    public static void SetFlagMarker(TemporaryMapMarker marker) => FlagMarker = marker;
    public static void RemoveFlagMarker()
    {
        AgentMap.Instance()->IsFlagMarkerSet = 0;
        FlagMarker = null;
    }
    
    public static void SetGatheringMarker(TemporaryMapMarker marker) => GatheringMarker = marker;
    public static void RemoveGatheringMarker() => GatheringMarker = null;
}