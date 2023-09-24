using System.Drawing;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using Mappy.Interfaces;
using Mappy.System;
using Mappy.System.Localization;
using Mappy.Views.General;

namespace Mappy.Views.Components;

public class MapSearchWidget : IMapSearchWidget
{
    public bool ShowMapSelectOverlay { get; set; }
    private static bool UseRegionView => MappySystem.SystemConfig.UseRegionSearch;

    private readonly DefaultIconSfxButton mapSelectButton;
    private readonly MapSearchView searchView;
    private readonly MapRegionView regionView;

    public MapSearchWidget()
    {
        searchView = new MapSearchView(this);
        regionView = new MapRegionView(this);
        
        mapSelectButton = new DefaultIconSfxButton
        {
            ClickAction = () =>
            {
                ShowMapSelectOverlay = !ShowMapSelectOverlay;

                if (UseRegionView) regionView.Show();
                if (!UseRegionView) searchView.Show();
            },
            Label = FontAwesomeIcon.Search.ToIconString() + "##MapSelectButton",
            TooltipText = Strings.SearchForMap,
            Size = ImGuiHelpers.ScaledVector2(26.0f, 23.0f),
        };
    }

    public void DrawWidget()
    {
        var showOverlay = ShowMapSelectOverlay;

        if (showOverlay) ImGui.PushStyleColor(ImGuiCol.Button, KnownColor.Red.Vector());
        mapSelectButton.Draw();
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

        if (UseRegionView)
        {
            regionView.Draw();
        }
        else
        {
            searchView.Draw();
        }
    }
}