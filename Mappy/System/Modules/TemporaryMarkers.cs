using System.Drawing;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
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
    
    public static TemporaryMapMarker? TempMapMarker { get; private set; }

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (TempMapMarker is null) return false;
        if (TempMapMarker.MapID != map.RowId) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        if (TempMapMarker is null) return;
        var config = GetConfig<TemporaryMarkersConfig>();

        TempMapMarker.DrawRing(viewport, map, config.CircleColor);
        if(config.ShowIcon) TempMapMarker.DrawIcon(map, config.IconScale);
        if(config.ShowTooltip) TempMapMarker.DrawTooltip(viewport, map, config.TooltipColor);
        
        ShowContextMenu(viewport, map);
    }

    private void ShowContextMenu(Viewport viewport, Map map)
    {
        if (!ImGui.IsMouseClicked(ImGuiMouseButton.Right)) return;
        if (TempMapMarker is null) return;

        var markerLocation = Position.GetTextureOffsetPosition(TempMapMarker.Position, map);
        var markerScreePosition = markerLocation * viewport.Scale + viewport.StartPosition - viewport.Offset;
        var cursorLocation = ImGui.GetMousePos();

        if (Vector2.Distance(markerScreePosition, cursorLocation) > TempMapMarker.Radius * viewport.Scale) return;

        var contextType = TempMapMarker.Type switch
        {
            MarkerType.Flag => ContextMenuType.Flag,
            MarkerType.Gathering => ContextMenuType.GatheringArea,
            MarkerType.Command => ContextMenuType.Command,
            MarkerType.Quest => ContextMenuType.Quest,
            _ => ContextMenuType.Inactive
        };
        
        MappySystem.ContextMenuController.Show(contextType, viewport, map);
    }
    
    public static void SetMarker(TemporaryMapMarker marker) => TempMapMarker = marker;
    public static void RemoveMarker()
    {
        if (TempMapMarker?.Type is MarkerType.Flag)
        {
            AgentMap.Instance()->IsFlagMarkerSet = 0;
        }

        TempMapMarker = null;
    }
}