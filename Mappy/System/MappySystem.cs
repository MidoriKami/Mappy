using DailyDuty.System;
using Dalamud.Game;
using KamiLib;
using Mappy.Models;
using Mappy.Utility;
using Mappy.Views.Windows;

namespace Mappy.System;

public class MappySystem
{
    public static SystemConfig SystemConfig = null!;
    public static ModuleController ModuleController = null!;
    public static MapTextureController MapTextureController = null!;
    public static PenumbraController PenumbraController = null!;
    public static GameIntegration GameIntegration = null!;
    public static ContextMenuController ContextMenuController = null!;
    
    public MappySystem()
    {
        SystemConfig = new SystemConfig();
        SystemConfig = LoadConfig();

        MapTextureController = new MapTextureController();
        ModuleController = new ModuleController();
        PenumbraController = new PenumbraController();
        GameIntegration = new GameIntegration();
        ContextMenuController = new ContextMenuController();
        
        Service.ClientState.TerritoryChanged += ZoneChanged;
        Service.Framework.Update += FrameworkUpdate;
    }

    private void ZoneChanged(object? sender, ushort newZone)
    {
        ModuleController.ZoneChanged(newZone);
    }
    
    private void FrameworkUpdate(Framework framework)
    {
        if (!Service.ClientState.IsLoggedIn) return;
        if (Service.ClientState.IsPvP) return;
        
        GameIntegration.Update();
        
        MapTextureController.Update();
        
        ProcessFollowPlayer();
    }
    
    private static void ProcessFollowPlayer()
    {
        if (MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { } mapWindow) return;
        if (Service.ClientState.LocalPlayer is not { } player) return;

        if (SystemConfig.FollowPlayer)
        {
            mapWindow.Viewport.SetViewportCenter(Position.GetObjectPosition(player.Position, map));
        }
    }

    public void Load()
    {
        ModuleController.Load();
    }
    
    public void Unload()
    {
        Service.ClientState.TerritoryChanged -= ZoneChanged;
        Service.Framework.Update -= FrameworkUpdate;

        MapTextureController.Dispose();
        ModuleController.Unload();
        GameIntegration.Dispose();
    }

    private SystemConfig LoadConfig() => FileController.LoadFile<SystemConfig>("System.config.json", SystemConfig);
    public void SaveConfig() => FileController.SaveFile("System.config.json", SystemConfig.GetType(), SystemConfig);
}