using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface.Utility;
using ImGuiNET;
using Mappy.Abstracts;
using Mappy.Interfaces;
using Mappy.System.Localization;
using Mappy.Utility;

namespace Mappy.Views.General;

public class MapSearchView {
    private List<ISearchResult>? searchResults;
    private bool shouldFocusMapSearch;
    private string searchString = string.Empty;

    private static Vector2 RegionSize => ImGui.GetContentRegionMax();
    private static float SearchWidth => RegionSize.X * 2.0f / 3.0f;
    private static float SearchPositionY => MathF.Max(RegionSize.Y / 6.0f, 45.0f * ImGuiHelpers.GlobalScale);
    private static float SearchPositionX => RegionSize.X / 6.0f;
    private static Vector2 SearchPosition => new(SearchPositionX, SearchPositionY);

    private readonly IMapSearchWidget widget;
    
    public MapSearchView(IMapSearchWidget searchWidget) {
        widget = searchWidget;
    }
    
    public void Show() {
        shouldFocusMapSearch = true;
        Task.Run(SearchTask);
    }

    public void Draw() {
        ProcessEnterKey();

        DrawBackground();
        DrawSearchBox();
        DrawSearchResults();
    }
    
    private void SearchTask() => searchResults = MapSearch.Search(searchString);

    private void DrawSearchBox() {
        ImGui.SetCursorPos(SearchPosition);
        ImGui.PushItemWidth(SearchWidth);

        if (shouldFocusMapSearch) {
            ImGui.SetKeyboardFocusHere();
            shouldFocusMapSearch = false;
        }

        if (ImGui.InputTextWithHint("###MapSearch", Strings.SearchHint, ref searchString, 60, ImGuiInputTextFlags.AutoSelectAll)) {
            Service.Log.Debug("Refreshing Search Results");
            Task.Run(SearchTask);
        }
    }

    private void DrawSearchResults() {
        ImGui.SetCursorPos(SearchPosition + ImGuiHelpers.ScaledVector2(0.0f, 30.0f));
        if (ImGui.BeginChild("###SearchResultsChild", new Vector2(SearchWidth, RegionSize.Y * 3.0f / 4.0f))) {
            if (searchResults is not null) {
                ImGuiClip.ClippedDraw(searchResults, DrawEntry, 24.0f);
            }
        }
        ImGui.EndChild();
    }

    private void DrawEntry(ISearchResult result) {
        if (result.DrawEntry()) {
            widget.ShowMapSelectOverlay = false;
        }
    }

    private void DrawBackground() {
        var drawStart = ImGui.GetWindowPos();
        var drawStop = drawStart + ImGui.GetWindowSize();
        var backgroundColor = ImGui.GetColorU32(Vector4.Zero with { W = 0.8f });

        ImGui.GetWindowDrawList().AddRectFilled(drawStart, drawStop, backgroundColor);
    }
    
    private void ProcessEnterKey() {
        if (!ImGui.IsKeyPressed(ImGuiKey.Enter) & !ImGui.IsKeyPressed(ImGuiKey.KeypadEnter)) return;
        
        if (searchResults?.FirstOrDefault() is { } validResult) {
            validResult.Invoke();
            widget.ShowMapSelectOverlay = false;
        }
    }
}