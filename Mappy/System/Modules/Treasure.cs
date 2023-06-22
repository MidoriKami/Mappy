using System.Drawing;
using System.Numerics;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Views.Attributes;

namespace Mappy.System.Modules;

public class TreasureConfig : IconModuleConfigBase
{
    [ColorConfigOption("TooltipColor", "ModuleColors", 1, 255, 255, 255, 255)]
    public Vector4 TooltipColor = KnownColor.White.AsVector4();

    [IconSelection(null, "IconSelection", 2, 60003, 60354)]
    public uint SelectedIcon = 60003;
}

public class Treasure : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.TreasureMarkers;
    public override ModuleConfigBase Configuration { get; protected set; } = new TreasureConfig();
    
    public override void LoadForMap(MapData mapData)
    {
        
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        
    }
}