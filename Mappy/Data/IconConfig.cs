using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using KamiLib.Configuration;

namespace Mappy.Data;

public class IconSetting {
    public required uint IconId { get; set; }
    public bool Hide;
    public bool AllowTooltip = true;
    public float Scale = 1.0f;
    public bool AllowClick = true;
    public Vector4 Color = KnownColor.White.Vector();

    public void Reset() {
        Hide = false;
        AllowTooltip = true;
        Scale = 1.0f;
        AllowClick = true;
        Color = KnownColor.White.Vector();
    }
}

public class IconConfig {
    public Dictionary<uint, IconSetting> IconSettingMap = [];
    
    public static IconConfig Load() 
        => Service.PluginInterface.LoadConfigFile("Icons.config.json", () => new IconConfig());

    public void Save() {
        // Purge icon that should now be filtered
        IconSettingMap.Remove(60091);
        
        Service.PluginInterface.SaveConfigFile("Icons.config.json", System.IconConfig);
    } 
}
