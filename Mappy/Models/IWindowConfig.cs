using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

[Category("WindowOptions", 0)]
public interface IWindowConfig
{
    [BoolConfig("KeepOpen")]
    public bool KeepOpen { get; set; }
    
    [BoolConfig("FollowPlayer")]
    public bool FollowPlayer { get; set; }
    
    [BoolConfig("LockWindow")]
    public bool LockWindow { get; set; }
    
    [BoolConfig("HideWindowFrame")]
    public bool HideWindowFrame { get; set; }
    
    [BoolConfig("FadeWhenUnfocused")]
    public bool FadeWhenUnfocused { get; set; }
    
    [BoolConfig("AlwaysShowToolbar")]
    public bool AlwaysShowToolbar { get; set; }

    [BoolConfig("ShowToolbarOnHover")]
    public bool ShowToolbarOnHover { get; set; }
    
    [FloatConfig("FadePercent")]
    public float FadePercent { get; set; }

    [FloatConfig("ZoomSpeed", 0.05f, 0.40f)]
    public float ZoomSpeed { get; set; }
}