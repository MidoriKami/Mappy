using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Logging;
using KamiLib.AutomaticUserInterface;
using Mappy.Models.Enums;

namespace Mappy.Models;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 2;

    [BoolConfigOption("KeepOpen", "WindowOptions", 0)]
    public bool KeepOpen = true;
    
    [BoolConfigOption("FollowPlayer", "WindowOptions", 0)]
    public bool FollowPlayer = false;
    
    [BoolConfigOption("LockWindow", "WindowOptions", 0)]
    public bool LockWindow = false;
    
    [BoolConfigOption("HideWindowFrame", "WindowOptions", 0)]
    public bool HideWindowFrame = false;
    
    [BoolConfigOption("FadeWhenUnfocused", "WindowOptions", 0)]
    public bool FadeWhenUnfocused = true;
    
    [BoolConfigOption("AlwaysShowToolbar", "WindowOptions", 0)]
    public bool AlwaysShowToolbar = false;
    
    [FloatConfigOption("FadePercent", "WindowOptions", 0)]
    public float FadePercent = 0.60f;
    
    [BoolConfigOption("EnableIntegrations", "GameIntegrations", 1)]
    public bool EnableIntegrations = true;
    
    [BoolConfigOption("HideBetweenAreas", "DisplayOptions", 2)]
    public bool HideBetweenAreas = false;
    
    [BoolConfigOption("HideInDuties", "DisplayOptions", 2)]
    public bool HideInDuties = false;
    
    [BoolConfigOption("HideInCombat", "DisplayOptions", 2)]
    public bool HideInCombat = false;

    public Dictionary<ModuleName, ModuleConfigBase> ModuleConfigurations = new();

    public void Save()
    {
        PluginLog.Debug("Saving Configuration.");
        Service.PluginInterface.SavePluginConfig(this);
    }
}