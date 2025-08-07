using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace Mappy.Classes;

public static class MappyGuiTweaks {
    public static bool IconButton(FontAwesomeIcon icon, string id, string? tooltip) {
        using var imRaiiId = ImRaii.PushId(id);

        bool result;

        using (Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push()) {
            result = ImGui.Button($"{icon.ToIconString()}");
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && tooltip is not null) {
            ImGui.SetTooltip(tooltip);
        }

        return result;
    }
}