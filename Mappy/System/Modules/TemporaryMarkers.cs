using System.Drawing;
using System.Numerics;
using Dalamud.Utility;
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

        var iconId = TempMapMarker.IconID;
        var rawPosition = TempMapMarker.Position;
        var position = Position.GetTextureOffsetPosition(rawPosition, map);

        switch (TempMapMarker.Type)
        {
            case MarkerType.Gathering:
            case MarkerType.Quest:
            case MarkerType.Command:
                DrawRing(viewport, map);
                break;
        }
       
        if (GetConfig<TemporaryMarkersConfig>().ShowIcon) DrawUtilities.DrawIcon(iconId, position, config.IconScale);

        switch (TempMapMarker.Type)
        {
            case MarkerType.Gathering:
            case MarkerType.Quest:
            case MarkerType.Command:
                DrawTooltip();
                break;
        }
        
        ShowContextMenu();
    }

    private void ShowContextMenu()
    {
        if (!ImGui.IsItemClicked(ImGuiMouseButton.Right)) return;

        var contextType = TempMapMarker?.Type switch
        {
            MarkerType.Flag => ContextMenuType.Flag,
            MarkerType.Gathering => ContextMenuType.GatheringArea,
            MarkerType.Command => ContextMenuType.Command,
            MarkerType.Quest => ContextMenuType.Quest,
            _ => ContextMenuType.Inactive
        };
        
        MappySystem.ContextMenuController.Show(contextType);
    }
    
    private void DrawRing(Viewport viewport, Map map)
    {
        if (TempMapMarker is null) return;
        
        var config = GetConfig<TemporaryMarkersConfig>();

        var markerPosition = Position.GetTextureOffsetPosition(TempMapMarker.Position, map);
        var drawPosition = viewport.GetImGuiWindowDrawPosition(markerPosition);

        var radius = TempMapMarker.Radius * viewport.Scale;
        var color = ImGui.GetColorU32(config.CircleColor);
        
        ImGui.GetWindowDrawList().AddCircleFilled(drawPosition, radius, color);
        ImGui.GetWindowDrawList().AddCircle(drawPosition, radius, color, 0, 4);
    }
        
    private void DrawTooltip()
    {
        if (TempMapMarker is null) return;
        if (TempMapMarker.TooltipText.IsNullOrEmpty()) return;
        if (!ImGui.IsItemHovered()) return;
        
        var config = GetConfig<TemporaryMarkersConfig>();
        if(config.ShowTooltip) DrawUtilities.DrawTooltip(TempMapMarker.TooltipText, config.TooltipColor, TempMapMarker.IconID);
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