using System;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;
using Mappy.Utility;

namespace Mappy.System.Modules;

public class PluginIntegrations : ModuleBase {
    public override ModuleName ModuleName => ModuleName.PluginIntegrations;
    public override IModuleConfig Configuration { get; protected set; } = new PluginIntegrationsConfig();

    protected override void UpdateMarkers(Viewport viewport, Map map) {
        var windowPos = ImGui.GetWindowPos();
        
        foreach (var (_, marker) in IpcController.LineMarkers) {
            if (map.RowId != marker.MapId) continue;

            var start = windowPos + marker.Start * viewport.Scale - viewport.Offset;
            var stop = windowPos + marker.End * viewport.Scale - viewport.Offset;
            
            ImGui.GetWindowDrawList().AddLine(start, stop, ImGui.GetColorU32(marker.Color), marker.Thickness);
        }
        
        foreach (var (markerId, marker) in IpcController.Markers) {
            if (map.RowId != marker.MapId) continue;

            switch (marker.Type) {
                case IpcMarkerType.Image:
                    UpdateIcon(markerId, () => new MappyMapIcon {
                        MarkerId = markerId,
                        IconId = marker.IconId,
                        TexturePosition = marker.PositionType is PositionType.Texture ? marker.Position : null,
                        ObjectPosition = marker.PositionType is PositionType.World ? marker.Position : null,
                        Tooltip = marker.Tooltip,
                        TooltipExtraText = marker.Description,
                    }, icon => {
                        icon.TexturePosition = marker.PositionType is PositionType.Texture ? marker.Position : null;
                        icon.ObjectPosition = marker.PositionType is PositionType.World ? marker.Position : null;
                    });
                    break;
                
                case IpcMarkerType.Shape:
                    var position = marker.PositionType switch {
                        PositionType.Texture => marker.Position,
                        PositionType.World => Position.GetTexturePosition(marker.Position, map),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    
                    var center = windowPos + position * viewport.Scale - viewport.Offset;
                    
                    ImGui.GetWindowDrawList().AddCircleFilled(center, marker.Radius, ImGui.GetColorU32(marker.FillColor), marker.Segments);
                    ImGui.GetWindowDrawList().AddCircle(center, marker.Radius, ImGui.GetColorU32(marker.OutlineColor), marker.Segments);
                    break;
            }
        }
    }
}