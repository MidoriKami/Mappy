using DailyDuty.System;
using DailyDuty.Views.Windows;
using Dalamud.Plugin;
using KamiLib;
using KamiLib.Commands;
using Mappy.System.Localization;

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

        CommandController.RegisterMainCommand("/mappy");
        
        KamiCommon.WindowManager.AddConfigurationWindow(new ConfigurationWindow());
    }

    public void Dispose()
    {
        KamiCommon.Dispose();
        
        System.Dispose();
    }
}