using System.Drawing;
using System.Numerics;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;

namespace Mappy.System.Modules;

public class GatheringAreaConfig : IconModuleConfigBase
{
    [ColorConfigOption("CircleColor", "ModuleColors", 1, 0.0f, 0.0f, 1.0f, 0.33f)]
    public Vector4 CircleColor = new(0.0f, 0.0f, 1.0f, 0.33f);
    
    [ColorConfigOption("TooltipColor", "ModuleColors", 1, 1.0f, 1.0f, 1.0f, 1.0f)]
    public Vector4 TooltipColor = KnownColor.White.AsVector4();
}

public class GatheringArea : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.GatheringArea;
    public override ModuleConfigBase Configuration { get; protected set; } = new GatheringAreaConfig();
        
    public override void LoadForMap(MapData mapData)
    {
        
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        
    }
}