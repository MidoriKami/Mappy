using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;

namespace Mappy.System.Modules;

public class QuestConfig : IconModuleConfigBase
{
    
}

public class Quest : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.QuestMarkers;
    public override ModuleConfigBase Configuration { get; protected set; } = new QuestConfig();
    
    public override void LoadForMap(uint newMapId)
    {
        
    }
    
    protected override void DrawMarkers()
    {
        
    }
}