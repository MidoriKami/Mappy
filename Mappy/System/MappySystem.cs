using DailyDuty.System;
using Mappy.Models;

namespace Mappy.System;

public class MappySystem
{
    public static SystemConfig SystemConfig = null!;
    public static ModuleController ModuleController = null!;
    
    public MappySystem()
    {
        SystemConfig = new SystemConfig();
        SystemConfig = LoadConfig();
        
        ModuleController = new ModuleController();
    }

    public void Load()
    {
        ModuleController.Load();
    }
    
    public void Unload()
    {
        ModuleController.Unload();
    }

    private SystemConfig LoadConfig() => FileController.LoadFile<SystemConfig>("System.config.json", SystemConfig);
    public void SaveConfig() => FileController.SaveFile("System.config.json", SystemConfig.GetType(), SystemConfig);
}