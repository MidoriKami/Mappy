using System;
using Mappy.Models;

namespace Mappy.System;

public class MappySystem : IDisposable
{
    public static Configuration Config = null!;
    public static ModuleController ModuleController = null!;
    
    public MappySystem()
    {
        Config = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Config.Save();

        ModuleController = new ModuleController();
    }
    
    public void Dispose() => Unload();

    private void Load()
    {
        ModuleController.Load();
    }

    private void Unload()
    {
        ModuleController.Unload();
    }
}