using Dalamud.Plugin;
using KamiLib;
using KamiLib.Commands;
using Mappy;
using Mappy.System;
using Mappy.System.Localization;
using Mappy.Views.Windows;

namespace DailyDuty;

public sealed class MappyPlugin : IDalamudPlugin
{
    public string Name => "Mappy";

    public static MappySystem System = null!;
    
    public MappyPlugin(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        
        KamiCommon.Initialize(pluginInterface, Name);
        KamiCommon.RegisterLocalizationHandler(key => Strings.ResourceManager.GetString(key, Strings.Culture));
                
        System = new MappySystem();
        System.Load();

        CommandController.RegisterMainCommand("/mappy");
        
        KamiCommon.WindowManager.AddConfigurationWindow(new ConfigurationWindow());
    }

    public void Dispose()
    {
        KamiCommon.Dispose();
        
        System.Unload();
    }
}