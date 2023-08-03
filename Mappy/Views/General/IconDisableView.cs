using System.Drawing;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using KamiLib.Caching;
using KamiLib.Utilities;
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
        
        if (!seenIcons.Any()) ImGui.TextColored(KnownColor.Orange.AsVector4(), Strings.IconDisableNoIcons);

        var areaSize = ImGui.GetContentRegionMax().X;
        var itemSize = 64.0f + ImGui.GetStyle().ItemSpacing.X;
        var maxIconsPerRow = (int) (areaSize / itemSize);
        var currentIndex = 0;
        
        foreach (var icon in seenIcons.OrderBy(id => id))
        {
            DrawIcon(icon);

            if (++currentIndex < maxIconsPerRow) ImGui.SameLine();
            else currentIndex = 0;
        }
    }

    private void DrawIcon(uint iconId)
    {
        if (IconCache.Instance.GetIcon(iconId) is not { } iconTexture) return;
        if (MappySystem.SystemConfig is not { DisallowedIcons: var disallowedIcons }) return;

        var disabled = disallowedIcons.Contains(iconId);
        var color = ImGui.GetColorU32(disabled ? KnownColor.Red.AsVector4() : KnownColor.ForestGreen.AsVector4());
        var iconSize = new Vector2(64.0f, 64.0f);

        var start = ImGui.GetCursorScreenPos();
        var stop = start + iconSize;
        
        ImGui.Image(iconTexture.ImGuiHandle, iconSize);
        ImGui.GetWindowDrawList().AddRect(start, stop, color, 5.0f, ImDrawFlags.None, 3.0f);

        if (ImGui.IsItemClicked())
        {
            if (disabled) disallowedIcons.Remove(iconId);
            else disallowedIcons.Add(iconId);
            MappyPlugin.System.SaveConfig();
        }
    }
}