using System;
using KamiLib.CommandManager;
using KamiLib.Window;
using Mappy.Classes;
using Mappy.Data;
using Mappy.Windows;

namespace Mappy.Controllers;

public class MappyController : IDisposable {
    public MappyController() {
        System.SystemConfig = SystemConfig.Load();
        System.IconConfig = IconConfig.Load();
        
        System.CommandManager = new CommandManager(Service.PluginInterface, "mappy");

        System.MapRenderer = new MapRenderer();

        System.ConfigWindow = new ConfigurationWindow();
        System.MapWindow = new MapWindow();
        
        System.WindowManager = new WindowManager(Service.PluginInterface);
        System.WindowManager.AddWindow(System.ConfigWindow, WindowFlags.IsConfigWindow | WindowFlags.RequireLoggedIn);
        System.WindowManager.AddWindow(System.MapWindow, WindowFlags.RequireLoggedIn);

        System.IntegrationsController = new IntegrationsController();

        Service.PluginInterface.UiBuilder.OpenMainUi += OpenMapWindow;
        Service.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigWindow;
    }

    private void OpenMapWindow() 
        => System.MapWindow.UnCollapseOrToggle();

    private void OpenConfigWindow()
        => System.ConfigWindow.Toggle();

    public void Dispose() {
        System.WindowManager.Dispose();
        System.IntegrationsController.Dispose();
        
        Service.PluginInterface.UiBuilder.OpenMainUi -= OpenMapWindow;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigWindow;
    }
}