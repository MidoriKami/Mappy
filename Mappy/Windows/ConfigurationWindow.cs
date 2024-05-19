using System.Numerics;
using ImGuiNET;
using KamiLib.CommandManager;
using KamiLib.Window;

namespace Mappy.Windows;

public class ConfigurationWindow : Window {
    public ConfigurationWindow() : base("Mappy Configuration Window", new Vector2(400.0f, 300.0f), true) {
        System.CommandManager.RegisterCommand(new CommandHandler {
            Delegate = _ => System.ConfigWindow.Toggle(),
            ActivationPath = "/",
        });
    }
    
    protected override void DrawContents() {
        var configChanged = ImGui.Checkbox("Use LinearZoom", ref System.SystemConfig.UseLinearZoom);
        configChanged |= ImGui.SliderFloat("Zoom Speed", ref System.SystemConfig.ZoomSpeed, 0.001f, 0.500f);
        configChanged |= ImGui.SliderFloat("Icon Scale", ref System.SystemConfig.IconScale, 0.10f, 3.0f);
        configChanged |= ImGui.Checkbox("Show Misc Tooltips", ref System.SystemConfig.ShowMiscTooltips);

        if (configChanged) {
            System.SystemConfig.Save();
        }
    }
}