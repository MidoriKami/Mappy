using System.Collections.Generic;
using KamiLib.Configuration;

namespace Mappy.Data;

public class IconSetting {
    public required uint IconId { get; set; }
    public bool Hide;
    public bool AllowTooltip = true;
    public float Scale = 1.0f;
    public bool AllowClick = true;
}

public class IconConfig {
    public Dictionary<uint, IconSetting> IconSettingMap = [];
    
    public static IconConfig Load() 
        => Service.PluginInterface.LoadConfigFile("Icons.config.json", () => new IconConfig());

    public void Save() 
        => Service.PluginInterface.SaveConfigFile("Icons.config.json", System.IconConfig);
}
