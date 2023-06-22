using System.Drawing;
using System.Numerics;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;

namespace Mappy.System.Modules;

public class FateConfig : IconModuleConfigBase
{
    [BoolConfigOption("ShowRing", "ModuleConfig", 0)]
    public bool ShowRing = true;
    
    [BoolConfigOption("ExpiringWarning", "ModuleConfig", 0)]
    public bool ExpiringWarning = false;

    [IntCounterConfigOption("EarlyWarningTime", "ModuleConfig", 0, false)]
    public int EarlyWarningTime = 300;
    
    [ColorConfigOption("TooltipColor", "ModuleColors", 1, 1.0f, 1.0f, 1.0f, 1.0f)]
    public Vector4 TooltipColor = KnownColor.White.AsVector4();
    
    [ColorConfigOption("CircleColor", "ModuleColors", 1, 0.58f, 0.388f, 0.827f, 0.33f)]
    public Vector4 CircleColor = new(0.58f, 0.388f, 0.827f, 0.33f);

    [ColorConfigOption("ExpiringColor", "ModuleColors", 1, 1.0f, 0.0f, 0.0f, 0.33f)]
    public Vector4 ExpiringColor = KnownColor.Red.AsVector4() with { W = 0.33f };
}

public class Fates : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.FATEs;
    public override ModuleConfigBase Configuration { get; protected set; } = new FateConfig();
    
    public override void LoadForMap(uint newMapId)
    {
        
    }
    
    protected override void DrawMarkers()
    {
        
    }
}