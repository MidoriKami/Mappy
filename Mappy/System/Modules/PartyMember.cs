using System.Drawing;
using System.Numerics;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;

namespace Mappy.System.Modules;

public class PartyMemberConfig : IconModuleConfigBase
{
    [ColorConfigOption("TooltipColor", "ModuleColors", 1, 0, 0, 255, 255)]
    public Vector4 TooltipColor = KnownColor.Blue.AsVector4();
}

public class PartyMember : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.PartyMembers;
    public override ModuleConfigBase Configuration { get; protected set; } = new PartyMemberConfig();
    
    public override void LoadForMap(MapData mapData)
    {
        
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        
    }
}