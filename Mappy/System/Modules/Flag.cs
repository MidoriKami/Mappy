using System.Drawing;
using System.Numerics;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;

namespace Mappy.System.Modules;

public class FlagConfig : IconModuleConfigBase
{
    [ColorConfigOption("TooltipColor", "ModuleColors", 1, 1.0f, 1.0f, 1.0f, 1.0f)]
    public Vector4 TooltipColor = KnownColor.White.AsVector4();
}

public class Flag : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.FlagMarker;
    public override ModuleConfigBase Configuration { get; protected set; } = new FlagConfig();
        
    public override void LoadForMap(MapData mapData)
    {
        
    }

    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        
    }
}