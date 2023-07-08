using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

[Category("ToolbarOptions", 3)]
public interface IToolbarConfig
{
    [BoolConfig("AlwaysShowToolbar")]
    public bool AlwaysShowToolbar { get; set; }
    
    [BoolConfig("ShowToolbarOnHover")]
    public bool ShowToolbarOnHover { get; set; }
    
    [BoolConfig("FollowPlayer")]
    public bool FollowPlayer { get; set; }
    
    [BoolConfig("LockWindow")]
    public bool LockWindow { get; set; }
    
    [BoolConfig("HideWindowFrame")]
    public bool HideWindowFrame { get; set; }
}