using Mappy.Models.Enums;

namespace Mappy.Models;

public class SystemConfig : IWindowConfig, IGameIntegrationsConfig, IWindowDisplayConfig
{
    // IWindowConfig
    public bool KeepOpen { get; set; } = true;
    public bool FollowPlayer { get; set; } = true;
    public bool LockWindow { get; set; } = false;
    public bool HideWindowFrame { get; set; } = false;
    public FadeMode FadeMode { get; set; } = FadeMode.WhenUnFocused | FadeMode.WhenMoving;
    public bool AlwaysShowToolbar { get; set; } = false;
    public bool ShowToolbarOnHover { get; set; } = false;
    public float FadePercent { get; set; } = 0.60f;
    public float ZoomSpeed { get; set; } = 0.15f;
    
    // IGameIntegrationsConfig
    public bool EnableIntegrations  { get; set; } = true;
    public bool InsertFlagInChat  { get; set; } = true;
    
    // IWindowDisplayConfig
    public bool HideWithGameGui { get; set; } = true;
    public bool HideBetweenAreas { get; set; } = false;
    public bool HideInDuties { get; set; } = false;
    public bool HideInCombat { get; set; } = false;
}