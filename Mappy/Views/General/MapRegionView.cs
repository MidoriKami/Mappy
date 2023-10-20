using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface;
using ImGuiNET;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Interfaces;
using Mappy.Utility;

namespace Mappy.Views.General;

public class MapRegionView {
    private readonly IMapSearchWidget widget;

    private string selectedRegion = string.Empty;
    private string selectedMap = string.Empty;

    private HashSet<string> regions = new();
    private HashSet<string> maps = new();
    private IEnumerable<ISearchResult>? results;

    public MapRegionView(IMapSearchWidget searchWidget) {
        widget = searchWidget;
    }

    public void Show() {
        UpdateData();
    }
    
    public void Draw() {
        DrawBackground();
        
        ImGui.Columns(3);

        if (ImGui.BeginChild("RegionFrame")) {
            foreach (var region in regions) {
                if (ImGui.Selectable($"{region}##RegionResult", region == selectedRegion)) {
                    selectedRegion = selectedRegion == region ? string.Empty : region;
                    selectedMap = string.Empty;
                    UpdateData();
                }
            }
        }
        ImGui.EndChild();
        
        ImGui.NextColumn();
        if (ImGui.BeginChild("MapFrame")) {
            foreach (var map in maps) {
                if (ImGui.Selectable($"{map}##MapResult", selectedMap == map)) {
                    selectedMap = selectedMap == map ? string.Empty : map;
                    UpdateData();
                }
            }
        }
        ImGui.EndChild();

        ImGui.NextColumn();
        if (ImGui.BeginChild("PointOfInterestFrame")) {
            if (results is not null) {
                if (!results.Any()) {
                    ImGui.TextColored(KnownColor.Orange.Vector(), "No Results");
                }
                
                foreach (var result in results) {
                    if (result.DrawEntry()) {
                        widget.ShowMapSelectOverlay = false;
                    }
                }
            }
        }
        ImGui.EndChild();

        ImGui.Columns(1);
    }
    
    private void DrawBackground() {
        var drawStart = ImGui.GetWindowPos();
        var drawStop = drawStart + ImGui.GetWindowSize();
        var backgroundColor = ImGui.GetColorU32(Vector4.Zero with { W = 0.8f });

        ImGui.GetWindowDrawList().AddRectFilled(drawStart, drawStop, backgroundColor);
    }

    private void UpdateData() {
        Task.Run(() => {
            regions = LuminaCache<Map>.Instance
                .Where(map => map is { PlaceNameRegion.Value.Name.RawString: not "" })
                .Select(map => map.PlaceNameRegion.Value!.Name.RawString)
                .OrderBy(map => map)
                .ToHashSet();
        
            if (selectedRegion is not "") {
                maps = LuminaCache<Map>.Instance
                    .Where(map => map is { PlaceNameRegion.Value.Name.RawString: not "", PlaceName.Value.Name.RawString: not "" })
                    .Where(map => map.PlaceNameRegion.Value!.Name.RawString == selectedRegion)
                    .Select(map => map.PlaceName.Value!.Name.RawString)
                    .OrderBy(map => map)
                    .ToHashSet();
            } else {
                maps = new HashSet<string>();
            }

            if (selectedMap is not "") {
                var selectedMapInfo = LuminaCache<Map>.Instance.FirstOrDefault(map => map.PlaceName.Value?.Name.RawString == selectedMap);
                results = selectedMapInfo is null ? null : MapSearch.SearchByMapId(selectedMapInfo.RowId);
            } else {
                results = null;
            }
        });
    }
}