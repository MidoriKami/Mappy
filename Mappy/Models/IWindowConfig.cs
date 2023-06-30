using KamiLib.AutomaticUserInterface;
using Mappy.Models.Enums;

namespace Mappy.Models;

[Category("WindowOptions")]
public interface IWindowConfig
{
    [BoolConfig("KeepOpen")]
    public bool KeepOpen { get; set; }
    
    [BoolConfig("FollowPlayer")]
    public bool FollowPlayer { get; set; }
    
    [BoolConfig("CenterOnOpen", "CenterOnOpenHelp")]
    public bool CenterOnOpen { get; set; }
    
    [BoolConfig("LockWindow")]
    public bool LockWindow { get; set; }
    
    [BoolConfig("HideWindowFrame")]
    public bool HideWindowFrame { get; set; }
    
    [BoolConfig("AlwaysShowToolbar")]
    public bool AlwaysShowToolbar { get; set; }

    [BoolConfig("ShowToolbarOnHover")]
    public bool ShowToolbarOnHover { get; set; }
    
    [BoolConfig("AllowZoomOnHover")]
    public bool AllowZoomOnHover { get; set; }
    
    [FloatConfig("ZoomSpeed", 0.05f, 0.40f)]
    public float ZoomSpeed { get; set; }
    
    [FloatConfig("FadePercent")]
    public float FadePercent { get; set; }
    
    [EnumFlagsConfig("FadeMode")]
    public FadeMode FadeMode { get; set; }
}