using Dalamud.Plugin;
using Mappy.Controllers;

namespace Mappy;

public sealed class MappyPlugin : IDalamudPlugin {
    public static MappyController Controller = null!;
    
    public MappyPlugin(DalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Service>();
        
        Controller = new MappyController();
    }

    public void Dispose() {
        Controller.Dispose();
    }
}