using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

public class ModuleConfigBase
{
    [BoolConfigOption("Enable", "ModuleOptions", -3)]
    public bool Enable = true;

    [IntCounterConfigOption("Layer", "ModuleOptions", -3, "LayerHelp")]
    public int Layer = 1;
}