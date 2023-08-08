using System.Drawing;
using System.Numerics;
using ImGuiNET;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;

namespace Mappy.System.Modules;

[Category("ModuleConfig")]
public class PluginIntegrationsConfig : IModuleConfig, IIconConfig, ITooltipConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 11;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.White.AsVector4();
}

public class PluginIntegrations : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.PluginIntegrations;
    public override IModuleConfig Configuration { get; protected set; } = new PluginIntegrationsConfig();

    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<PluginIntegrationsConfig>();
        
        var windowPos = ImGui.GetWindowPos();
        
        foreach (var (_, marker) in IpcController.LineMarkers)
        {
            if (map.RowId != marker.MapId) continue;

            var start = windowPos + marker.Start * viewport.Scale - viewport.Offset;
            var stop = windowPos + marker.End * viewport.Scale - viewport.Offset;
            
            ImGui.GetWindowDrawList().AddLine(start, stop, ImGui.GetColorU32(marker.Color), marker.Thickness);
        }
        
        foreach (var (_, marker) in IpcController.Markers)
        {
            if (map.RowId != marker.MapId) continue;

            DrawUtilities.DrawMapIcon(new MappyMapIcon
            {
                IconId = marker.IconId,
                TexturePosition = marker.PositionType is PositionType.Texture ? marker.Position : null,
                ObjectPosition = marker.PositionType is PositionType.World ? marker.Position : null,
                IconScale = config.IconScale,
                ShowIcon = config.ShowIcon,
                
                Tooltip = marker.Tooltip,
                TooltipDescription = marker.Description,
                TooltipColor = config.TooltipColor,
                ShowTooltip = config.ShowTooltip,
                
            }, viewport, map);
        }
    }
}