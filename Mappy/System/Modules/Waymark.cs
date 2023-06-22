using KamiLib.AutomaticUserInterface;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;

namespace Mappy.System.Modules;

public class WaymarkConfig : IconModuleConfigBase
{
    [Disabled]
    public new bool ShowTooltip = false;
}

public class Waymark : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.Waymarks;
    public override ModuleConfigBase Configuration { get; protected set; } = new WaymarkConfig();
    
    public override void LoadForMap(uint newMapId)
    {
        
    }
    
    protected override void DrawMarkers()
    {
        
    }
}