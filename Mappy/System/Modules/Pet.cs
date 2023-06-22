using System.Drawing;
using System.Numerics;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;

namespace Mappy.System.Modules;

public class PetConfig : IconModuleConfigBase
{
    [ColorConfigOption("TooltipColor", "ModuleColors", 1, 128, 0, 128, 255)]
    public Vector4 TooltipColor = KnownColor.Purple.AsVector4();
}

public class Pet : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.Pets;
    public override ModuleConfigBase Configuration { get; protected set; } = new PetConfig();
    
    public override void LoadForMap(MapData mapData)
    {
        
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        
    }
}