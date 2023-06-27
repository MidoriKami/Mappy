using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

[Category("ModuleOptions", -10)]
public interface IModuleConfig
{
    [BoolConfig("Enable")]
    public bool Enable { get; set; }
    
    [IntCounterConfig("Layer")]
    public int Layer { get; set; }
}