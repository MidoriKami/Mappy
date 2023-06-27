using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using KamiLib.Utilities;
using Mappy.System;
using Mappy.System.Localization;
using Mappy.Utility;

namespace Mappy.Views.Components;

public class MapSelectWidget
{
    public bool ShowMapSelectOverlay { get; set; }
    
    private IEnumerable<SearchResult>? searchResults;
    private bool shouldFocusMapSearch;
    private string searchString = string.Empty;

    private DefaultIconSfxButton MapSelectButton;

    public MapSelectWidget()
    {
        MapSelectButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                ShowMapSelectOverlay = !ShowMapSelectOverlay;
                shouldFocusMapSearch = true;
            },
            Label = FontAwesomeIcon.Map.ToIconString() + "##MapSelectButton",
            TooltipText = Strings.SearchForMap,
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };
    }
    
    public void DrawWidget()
    {
        var showOverlay = ShowMapSelectOverlay;

        if (showOverlay) ImGui.PushStyleColor(ImGuiCol.Button, KnownColor.Red.AsVector4());
        MapSelectButton.Draw();
        if (showOverlay) ImGui.PopStyleColor();
    }
    
    public void Draw()
    {
        if (!ShowMapSelectOverlay) return;

        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
        {
            ShowMapSelectOverlay = false;
            return;
        }
        
        var searchWidth = 250.0f * ImGuiHelpers.GlobalScale;
        
        var drawStart = ImGui.GetWindowPos();
        var drawStop = drawStart + ImGui.GetWindowSize();
        var backgroundColor = ImGui.GetColorU32(Vector4.Zero with { W = 0.8f });
        
        ImGui.GetWindowDrawList().AddRectFilled(drawStart, drawStop, backgroundColor);

        var regionAvailable = ImGui.GetContentRegionAvail();
        var searchPosition = regionAvailable with {X = regionAvailable.X / 2.0f, Y = regionAvailable.Y / 4.0f};

        searchPosition = searchPosition with { Y = MathF.Max(searchPosition.Y, 40.0f * ImGuiHelpers.GlobalScale + 5.0f * ImGuiHelpers.GlobalScale)};
        ImGui.SetCursorPos(searchPosition - new Vector2(searchWidth / 2.0f, 0.0f));
        ImGui.PushItemWidth(searchWidth);
        
        if (shouldFocusMapSearch)
        {
            ImGui.SetKeyboardFocusHere();
            shouldFocusMapSearch = false;
        }
        
        if (ImGui.InputTextWithHint("###MapSearch", Strings.SearchHint, ref searchString, 60, ImGuiInputTextFlags.AutoSelectAll))
        {
            searchResults = MapSearch.Search(searchString, 10);
            PluginLog.Debug("Refreshing Search Results");
        }

        ImGui.SetCursorPos(searchPosition - new Vector2(searchWidth / 2.0f, 0.0f) + ImGuiHelpers.ScaledVector2(0.0f, 30.0f));
        if (ImGui.BeginChild("###SearchResultsChild", new Vector2(searchWidth, regionAvailable.Y * 3.0f / 4.0f )))
        {
            if (searchResults is not null)
            {
                foreach (var result in searchResults)
                {
                    ImGui.SetNextItemWidth(250.0f * ImGuiHelpers.GlobalScale);
                    if (ImGui.Selectable(result.Label))
                    {
                        MappySystem.MapTextureController.LoadMap(result.MapID);
                        ShowMapSelectOverlay = false;
                    }
                }
            }
        }
        ImGui.EndChild();
    }
}