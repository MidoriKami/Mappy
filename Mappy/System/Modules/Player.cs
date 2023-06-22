using KamiLib.AutomaticUserInterface;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;

namespace Mappy.System.Modules;

public class PlayerConfig : IconModuleConfigBase
{
    [Disabled]
    public new bool ShowTooltip = false;
    
    [BoolConfigOption("ShowCone", "ModuleConfig", 0)]
    public bool ShowCone = true;

    [FloatConfigOption("ConeRadius", "ModuleConfig", 0, 0.0f, 360.0f)]
    public float ConeRadius = 90.0f;

    [FloatConfigOption("ConeAngle", "ModuleConfig", 0, 0.0f, 180.0f)]
    public float ConeAngle = 90.0f;

    [FloatConfigOption("OutlineThickness", "ModuleConfig", 0, 0.5f, 5.0f)]
    public float OutlineThickness = 2.0f;
}

public class Player : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.Player;
    public override ModuleConfigBase Configuration { get; protected set; } = new PlayerConfig();
        
    public override void LoadForMap(uint newMapId)
    {
        
    }
    
    protected override void DrawMarkers()
    {
        
    }
}