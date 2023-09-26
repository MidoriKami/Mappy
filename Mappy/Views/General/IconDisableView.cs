using System;
using System.Drawing;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using KamiLib.Windows;
using Mappy.System;
using Mappy.System.Localization;

namespace Mappy.Views.General;

public class IconDisableView
{
    private int currentIndex;
    private int itemsPerLine;
    
    public void Draw()
    {
        if (MappySystem.SystemConfig is not { SeenIcons: var seenIcons }) return;

        ImGui.Text(Strings.IconDisableInfo);
        ImGui.Separator();
        
        if (!seenIcons.Any()) ImGui.TextColored(KnownColor.Orange.Vector(), Strings.IconDisableNoIcons);

        itemsPerLine = (int) MathF.Floor(ImGui.GetContentRegionAvail().X / (64.0f * ImGuiHelpers.GlobalScale + ImGui.GetStyle().ItemSpacing.X));
        
        currentIndex = 0;
        foreach (var icon in seenIcons.OrderBy(id => id).Where(id => id is not (>= 60483 and <= 60494) and not 0))
        {
            if (currentIndex >= itemsPerLine && currentIndex % itemsPerLine == 0) ImGui.NewLine();
            
            DrawIcon(icon);
        }
    }

    private void DrawIcon(uint iconId)
    {
        if (Service.TextureProvider.GetIcon(iconId) is not { } iconTexture) return;
        if (MappySystem.SystemConfig is not { DisallowedIcons: var disallowedIcons }) return;

        var disabled = disallowedIcons.Contains(iconId);
        var color = ImGui.GetColorU32(disabled ? KnownColor.Red.Vector() : KnownColor.ForestGreen.Vector());

        var size = ImGuiHelpers.ScaledVector2(64.0f, 64.0f);
        var start = ImGui.GetCursorScreenPos();
        var stop = start + size;
        
        ImGui.Image(iconTexture.ImGuiHandle, size);
        ImGui.GetWindowDrawList().AddRect(start, stop, color, 5.0f, ImDrawFlags.None, 3.0f);
        ImGui.SameLine();
        currentIndex++;

        if (ImGui.IsItemClicked())
        {
            if (disabled) disallowedIcons.Remove(iconId);
            else disallowedIcons.Add(iconId);
            MappyPlugin.System.SaveConfig();
        }
    }
}