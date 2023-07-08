using KamiLib.AutomaticUserInterface;
using KamiLib.AutomaticUserInterface.Configuration;
using Mappy.Models.Enums;

namespace Mappy.Models;

[Category("FadeOptions", 2)]
public interface IFadeConfig
{
    [EnumToggle("FadeMode")]
    public FadeMode FadeMode { get; set; }
    
    [FloatConfig("FadePercent")]
    public float FadePercent { get; set; }
}