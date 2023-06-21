using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

public class ModuleConfigBase
{
    [BoolConfigOption("Enable", "ModuleOptions", -1)]
    public bool Enable = true;
}