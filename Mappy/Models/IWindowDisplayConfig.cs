using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

[Category("DisplayOptions", 5)]
public interface IWindowDisplayConfig
{
    [BoolConfig("HideWithGameGui")] 
    public bool HideWithGameGui { get; set; }

    [BoolConfig("HideBetweenAreas")]
    public bool HideBetweenAreas { get; set; }
    
    [BoolConfig("HideInDuties")]
    public bool HideInDuties { get; set; }
    
    [BoolConfig("HideInCombat")]
    public bool HideInCombat { get; set; }
}