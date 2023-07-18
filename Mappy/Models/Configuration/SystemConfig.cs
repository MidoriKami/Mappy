using System.Numerics;
using Mappy.Models.Enums;

namespace Mappy.Models;

public class SystemConfig : IWindowConfig, IGameIntegrationsConfig, IWindowDisplayConfig, IToolbarConfig, IZoomConfig, IFadeConfig
{
    public bool KeepOpen { get; set; } = true;
    public bool IgnoreEscapeKey { get; set; } = false;
    public bool FollowPlayer { get; set; } = true;
    public bool CenterOnOpen { get; set; } = false;
    public Vector2 WindowPosition { get; set; } = new(1024.0f, 700.0f);
    public Vector2 WindowSize { get; set; } = new(500.0f, 500.0f);
    public bool LockWindow { get; set; } = false;
    public bool HideWindowFrame { get; set; } = false;
    public FadeMode FadeMode { get; set; } = FadeMode.WhenUnFocused | FadeMode.WhenMoving;
    public bool AlwaysShowToolbar { get; set; } = false;
    public bool ShowToolbarOnHover { get; set; } = true;
    public float FadePercent { get; set; } = 0.60f;
    public float ZoomSpeed { get; set; } = 0.15f;
    public bool ZoomInOnFlag { get; set; } = true;
    public bool EnableIntegrations  { get; set; } = true;
    public bool HideWithGameGui { get; set; } = true;
    public bool HideBetweenAreas { get; set; } = false;
    public bool HideInDuties { get; set; } = false;
    public bool HideInCombat { get; set; } = false;
}