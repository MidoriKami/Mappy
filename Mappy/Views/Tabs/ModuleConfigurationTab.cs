using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;
using KamiLib.Interfaces;
using Mappy.System;
using Mappy.System.Localization;
using Mappy.Views.Components;

namespace Mappy.Views.Tabs;

public class ModuleConfigurationTab : ISelectionWindowTab
{
    public string TabName => Strings.Modules;
    public ISelectable? LastSelection { get; set; }
    public IEnumerable<ISelectable> GetTabSelectables() => MappySystem.ModuleController.Modules.Select(module => new ModuleSelectable(module));

    public void DrawTabExtras()
    {
        var buttonSize = ImGuiHelpers.ScaledVector2(30.0f);
        var region = ImGui.GetContentRegionAvail();
        
        var cursorStart = ImGui.GetCursorPos();
        cursorStart.X += region.X / 2.0f - buttonSize.X / 2.0f;
        
        ImGui.SetCursorPos(cursorStart);
        
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFFC);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);

        if (ImGuiComponents.IconButton("KoFiButton", FontAwesomeIcon.Coffee)) Process.Start(new ProcessStartInfo { FileName = "https://ko-fi.com/midorikami", UseShellExecute = true });
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Support Me on Ko-Fi");
        
        ImGui.PopStyleColor(3);
    }
}