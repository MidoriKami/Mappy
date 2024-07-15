using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.CommandManager;
using KamiLib.Window;
using Mappy.Classes;
using Mappy.Controllers;
using Mappy.Data;
using Mappy.Windows;

namespace Mappy;

public sealed class MappyPlugin : IDalamudPlugin {
    public MappyPlugin(IDalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Service>();
        
        System.SystemConfig = SystemConfig.Load();
        System.IconConfig = IconConfig.Load();

        System.Teleporter = new Teleporter();
        
        System.CommandManager = new CommandManager(Service.PluginInterface, "mappy");

        System.MapRenderer = new MapRenderer.MapRenderer();

        System.ConfigWindow = new ConfigurationWindow();
        System.MapWindow = new MapWindow();
        
        System.WindowManager = new WindowManager(Service.PluginInterface);
        System.WindowManager.AddWindow(System.ConfigWindow, WindowFlags.IsConfigWindow | WindowFlags.RequireLoggedIn);
        System.WindowManager.AddWindow(System.MapWindow, WindowFlags.RequireLoggedIn);

        System.IntegrationsController = new IntegrationsController();

        Service.PluginInterface.UiBuilder.OpenMainUi += OpenMapWindow;
    }

    private unsafe void OpenMapWindow()
        => AgentMap.Instance()->Show();

    public void Dispose() {
        System.WindowManager.Dispose();
        System.IntegrationsController.Dispose();
        
        Service.PluginInterface.UiBuilder.OpenMainUi -= OpenMapWindow;
    }
}