using System;
using System.ComponentModel;
using KamiLib.Configuration;

namespace Mappy.Data;

public enum CenterTarget {
    [Description("Disabled")]
    Disabled = 0,

    [Description("Player")]
    Player = 1,

    [Description("Map")]
    Map = 2
}

[Flags]
public enum FadeMode {
    [Description("Always")]
    Always = 1 << 0,
    
    [Description("When Moving")]
    WhenMoving = 1 << 2,
    
    [Description("When Focused")]
    WhenFocused = 1 << 3,
    
    [Description("When Unfocused")]
    WhenUnFocused = 1 << 4,
}

public class SystemConfig : CharacterConfiguration {

    public bool UseLinearZoom = false;
    public float ZoomSpeed = 0.05f;
    public float IconScale = 0.50f;
    public bool ShowMiscTooltips = true;

    // public bool KeepOpen { get; set; } = false;
    // public bool IgnoreEscapeKey { get; set; } = false;
    // public bool FollowPlayer { get; set; } = true;
    // public bool FollowOnOpen { get; set; } = false;
    // public bool ShowMapName { get; set; } = true;
    // public CenterTarget CenterOnOpen { get; set; } = CenterTarget.Disabled;
    // public Vector2 WindowPosition { get; set; } = new(1024.0f, 700.0f);
    // public Vector2 WindowSize { get; set; } = new(500.0f, 500.0f);
    // public bool LockWindow { get; set; } = false;
    // public bool HideWindowFrame { get; set; } = false;
    // public FadeMode FadeMode { get; set; } = FadeMode.WhenUnFocused | FadeMode.WhenMoving;
    // public bool AlwaysShowToolbar { get; set; } = false;
    // public bool ShowToolbarOnHover { get; set; } = true;
    // public float FadePercent { get; set; } = 0.60f;
    // public float ZoomSpeed { get; set; } = 0.15f;
    // public bool ZoomInOnFlag { get; set; } = true;
    // public bool FocusObjective { get; set; } = true;
    // public bool EnableIntegrations  { get; set; } = true;
    // public bool IntegrationsUnCollapse { get; set; } = true;
    // public bool HideWithGameGui { get; set; } = true;
    // public bool HideBetweenAreas { get; set; } = false;
    // public bool HideInDuties { get; set; } = false;
    // public bool HideInCombat { get; set; } = false;
    //
    // public HashSet<uint> DisallowedIcons { get; set; } = [];
    // public HashSet<uint> SeenIcons { get; set; } = [];

    public static SystemConfig Load() 
        => Service.PluginInterface.LoadConfigFile("System.config.json", () => new SystemConfig());

    public void Save() 
        => Service.PluginInterface.SaveConfigFile("System.config.json", System.SystemConfig);
}