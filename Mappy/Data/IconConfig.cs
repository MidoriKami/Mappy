using System.Collections.Generic;
using KamiLib.Configuration;

namespace Mappy.Controllers;

public class IconSetting {
    public bool Hide { get; set; }
    public bool AllowTooltip { get; set; } = true;
    public float Scale { get; set; } = 1.0f;
}

public class IconConfig {
    public Dictionary<uint, IconSetting> IconSettingMap = [];
    
    public static IconConfig Load() 
        => Service.PluginInterface.LoadConfigFile("Icons.config.json", () => new IconConfig());

    public void Save() 
        => Service.PluginInterface.SaveConfigFile("Icons.config.json", System.IconConfig);
}
