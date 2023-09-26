using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using Mappy.System;
using Mappy.System.Localization;

namespace Mappy.Views.General;

public class IconDisableView
{
    public void Draw()
    {
        if (MappySystem.SystemConfig is not { SeenIcons: var seenIcons }) return;

        ImGui.Text(Strings.IconDisableInfo);
        ImGui.Separator();
        
        if (!seenIcons.Any()) ImGui.TextColored(KnownColor.Orange.Vector(), Strings.IconDisableNoIcons);

        var itemWidth = 64.0f * ImGuiHelpers.GlobalScale + ImGui.GetStyle().ItemSpacing.X;
        var itemHeight = 64.0f * ImGuiHelpers.GlobalScale + ImGui.GetStyle().ItemSpacing.Y;
        
        var itemsPerLine = (int) MathF.Floor(ImGui.GetContentRegionAvail().X / itemWidth);
        
        var filteredIcons = seenIcons
            .OrderBy(id => id)
            .Where(id => id is not (>= 60483 and <= 60494)) // Remove invisible icons representing area circles
            .Where(id => id is not 0) // Remove null icons
            .Where(id => id is not (>= 62620 and <= 62799)) // Remove special area icons, ie "Gold Saucer"
            .Where(id => id is not (>= 63201 and <= 063899)) // Remove "Region" icons
            .ToList();
        
        ClippedDraw(filteredIcons, DrawIcon, itemsPerLine, itemHeight);
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

        if (ImGui.IsItemClicked())
        {
            if (disabled) disallowedIcons.Remove(iconId);
            else disallowedIcons.Add(iconId);
            MappyPlugin.System.SaveConfig();
        }
        
        #if DEBUG
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip($"{iconId}");
        }
        #endif
    }
    
    public static void ClippedDraw<T>(IReadOnlyList<T> data, Action<T> draw, int itemsPerLine, float lineHeight)
    {
        ImGuiListClipperPtr clipper;
        unsafe
        {
            clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
        }
        
        var maxRows = (int) MathF.Ceiling((float)data.Count / itemsPerLine);
        
        clipper.Begin(maxRows, lineHeight);
        while (clipper.Step())
        {
            for (var actualRow = clipper.DisplayStart; actualRow < clipper.DisplayEnd; actualRow++)
            {
                if (actualRow >= maxRows)
                    return;

                if (actualRow < 0)
                    continue;

                var itemsForRow = data
                    .Skip(actualRow * itemsPerLine)
                    .Take(itemsPerLine);
                
                var currentIndex = 0;
                foreach (var item in itemsForRow)
                {
                    if (currentIndex++ != 0 && currentIndex < itemsPerLine + 1)
                    {
                        ImGui.SameLine();
                    }
                    
                    draw(item);
                }
            }
        }

        clipper.End();
        clipper.Destroy();
    }
}