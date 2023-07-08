using KamiLib.AutomaticUserInterface;

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
}