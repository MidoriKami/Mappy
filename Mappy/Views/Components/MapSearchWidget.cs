using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using KamiLib.Utilities;
using Mappy.Abstracts;
using Mappy.System.Localization;
using Mappy.Utility;

namespace Mappy.Views.Components;

public class MapSearchWidget
{
    public bool ShowMapSelectOverlay { get; set; }

    private IEnumerable<ISearchResult>? searchResults;
    private bool shouldFocusMapSearch;
    private string searchString = string.Empty;

    private readonly DefaultIconSfxButton mapSelectButton;

    public MapSearchWidget()
    {
        mapSelectButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                ShowMapSelectOverlay = !ShowMapSelectOverlay;
                shouldFocusMapSearch = true;
                Task.Run(SearchTask);
            },
            Label = FontAwesomeIcon.Map.ToIconString() + "##MapSelectButton",
            TooltipText = Strings.SearchForMap,
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };
    }

    private void SearchTask() => searchResults = MapSearch.Search(searchString, 10);

    public void DrawWidget()
    {
        var showOverlay = ShowMapSelectOverlay;

        if (showOverlay) ImGui.PushStyleColor(ImGuiCol.Button, KnownColor.Red.AsVector4());
        mapSelectButton.Draw();
        if (showOverlay) ImGui.PopStyleColor();
    }
    
    public void Draw()
    {
        ProcessEnterKey();
        if (!ShowMapSelectOverlay) return;

        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
        {
            ShowMapSelectOverlay = false;
            return;
        }
        
        var regionAvailable = ImGui.GetContentRegionAvail();
        var searchWidth = ImGui.GetContentRegionAvail().X * 2.0f / 3.0f;
        var searchPosition = regionAvailable with { X = regionAvailable.X / 6.0f, Y =  MathF.Max(regionAvailable.Y / 4.0f, 40.0f * ImGuiHelpers.GlobalScale + 5.0f * ImGuiHelpers.GlobalScale) };
        
        DrawBackground();

        DrawSearchBox(searchPosition, searchWidth);
        DrawSearchResults(searchPosition, searchWidth, regionAvailable);
    }
    
    private void ProcessEnterKey()
    {
        if (!ImGui.IsKeyPressed(ImGuiKey.Enter) & !ImGui.IsKeyPressed(ImGuiKey.KeypadEnter)) return;
        
        if (searchResults?.FirstOrDefault() is { } validResult)
        {
            validResult.Invoke();
            ShowMapSelectOverlay = false;
        }
    }

    private static void DrawBackground()
    {
        var drawStart = ImGui.GetWindowPos();
        var drawStop = drawStart + ImGui.GetWindowSize();
        var backgroundColor = ImGui.GetColorU32(Vector4.Zero with { W = 0.8f });

        ImGui.GetWindowDrawList().AddRectFilled(drawStart, drawStop, backgroundColor);
    }
    
    private void DrawSearchBox(Vector2 searchPosition, float searchWidth)
    {
        ImGui.SetCursorPos(searchPosition);
        ImGui.PushItemWidth(searchWidth);

        if (shouldFocusMapSearch)
        {
            ImGui.SetKeyboardFocusHere();
            shouldFocusMapSearch = false;
        }

        if (ImGui.InputTextWithHint("###MapSearch", Strings.SearchHint, ref searchString, 60, ImGuiInputTextFlags.AutoSelectAll))
        {
            PluginLog.Debug("Refreshing Search Results");
            Task.Run(SearchTask);
        }
    }
    
    private void DrawSearchResults(Vector2 searchPosition, float searchWidth, Vector2 regionAvailable)
    {
        ImGui.SetCursorPos(searchPosition + ImGuiHelpers.ScaledVector2(0.0f, 30.0f));
        if (ImGui.BeginChild("###SearchResultsChild", new Vector2(searchWidth, regionAvailable.Y * 3.0f / 4.0f)))
        {
            if (searchResults is not null)
            {
                foreach (var entry in searchResults)
                {
                    if (entry.DrawEntry())
                    {
                        ShowMapSelectOverlay = false;
                    }
                }
            }
        }
        ImGui.EndChild();
    }
}