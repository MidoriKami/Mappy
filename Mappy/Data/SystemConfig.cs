using System;
using System.ComponentModel;
using System.Numerics;
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
    public bool HideWithGameGui = true;
    public bool HideBetweenAreas = false;
    public bool HideInDuties = false;
    public bool HideInCombat = false;
    public bool KeepOpen = false;
    public bool FollowOnOpen = false;
    public bool FollowPlayer = true;
    public bool RememberLastMap = true;
    public uint LastMapId = 0;
    public CenterTarget CenterOnOpen = CenterTarget.Disabled;
    public bool ShowRadar = true;
    public bool HideWindowFrame = false;
    public bool IgnoreEscapeKey = false;
    public bool LockWindow = false;
    public float FadePercent = 0.60f;
    public FadeMode FadeMode = FadeMode.WhenUnFocused | FadeMode.WhenMoving;
    public Vector2 WindowPosition = new(1024.0f, 700.0f);
    public Vector2 WindowSize = new(500.0f, 500.0f);
    public bool AlwaysShowToolbar = false;
    public bool ShowToolbarOnHover = true;

    public static SystemConfig Load() 
        => Service.PluginInterface.LoadConfigFile("System.config.json", () => new SystemConfig());

    public void Save() 
        => Service.PluginInterface.SaveConfigFile("System.config.json", System.SystemConfig);
}