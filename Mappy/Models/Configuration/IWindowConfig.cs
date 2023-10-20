using System.Numerics;
using KamiLib.AutomaticUserInterface;
using Mappy.Models.Enums;

namespace Mappy.Models;

[Category("WindowOptions")]
public interface IWindowConfig {
    [BoolConfig("KeepOpen")]
    public bool KeepOpen { get; set; }
    
    [BoolConfig("IgnoreEscapeKey", "IgnoreEscapeKeyHelp")]
    public bool IgnoreEscapeKey { get; set; }
    
    [BoolConfig("FollowOnOpen", "FollowOnOpenHelp")]
    public bool FollowOnOpen { get; set; }
    
    [BoolConfig("ShowMapName")]
    public bool ShowMapName { get; set; }
    
    [RadioEnumConfig("CenterOnOpen", "CenterOnOpenHelp")]
    public CenterTarget CenterOnOpen { get; set; }
    
    [Vector2Config("WindowPosition", 1.0f)]
    public Vector2 WindowPosition { get; set; }
    
    [Vector2Config("WindowSize", 510, 200, 9999, 9999, 1.0f)]
    public Vector2 WindowSize { get; set; }
}