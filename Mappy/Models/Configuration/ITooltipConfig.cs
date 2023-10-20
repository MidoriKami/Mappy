using System.Numerics;
using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

[Category("TooltipConfig", -3)]
public interface ITooltipConfig {
    [BoolConfig("ShowTooltip")]
    public bool ShowTooltip { get; set; }
    
    [ColorConfig("TooltipColor")]
    public Vector4 TooltipColor { get; set; }
}