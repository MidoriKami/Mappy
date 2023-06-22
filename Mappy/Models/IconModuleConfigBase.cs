using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

public class IconModuleConfigBase : ModuleConfigBase
{
    [BoolConfigOption("ShowIcon", "IconConfig", -2)]
    public bool ShowIcon = true;
    
    [BoolConfigOption("ShowTooltip", "IconConfig", -2)]
    public bool ShowTooltip = true;
    
    [FloatConfigOption("IconScale", "IconConfig", -2)]
    public float IconScale = 0.5f;
}