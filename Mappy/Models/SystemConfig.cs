using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

public class SystemConfig
{
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

    [BoolConfigOption("ShowToolbarOnHover", "WindowOptions", 0)]
    public bool ShowToolbarOnHover = false;
    
    [FloatConfigOption("FadePercent", "WindowOptions", 0)]
    public float FadePercent = 0.60f;

    [FloatConfigOption("ZoomSpeed", "WindowOptions", 0, 0.05f, 0.40f)]
    public float ZoomSpeed = 0.15f;
    
    [BoolDescriptionConfigOption("EnableIntegrations", "GameIntegrations", 1, "IntegrationsDescription")]
    public bool EnableIntegrations = true;
    
    [BoolConfigOption("InsertFlagInChat", "GameIntegrations", 1, "InsertFlagHelp")]
    public bool InsertFlagInChat = true;
    
    [BoolConfigOption("HideBetweenAreas", "DisplayOptions", 2)]
    public bool HideBetweenAreas = false;
    
    [BoolConfigOption("HideInDuties", "DisplayOptions", 2)]
    public bool HideInDuties = false;
    
    [BoolConfigOption("HideInCombat", "DisplayOptions", 2)]
    public bool HideInCombat = false;
}