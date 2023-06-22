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

public class AllianceConfig : IconModuleConfigBase
{
    [ColorConfigOption("TooltipColor", "ModuleConfig", 0, 0.133f, 0.545f, 0.133f, 1.0f)]
    public Vector4 TooltipColor = KnownColor.ForestGreen.AsVector4();
    
    [IconSelection(null, "IconSelection", 0, 60358, 60359, 60360, 60361)]
    public uint SelectedIcon = 60358;
}

public class AllianceMember : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.AllianceMembers;
    public override ModuleConfigBase Configuration { get; protected set; } = new AllianceConfig();
    
    public override void LoadForMap(MapData mapData)
    {
        
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        
    }
}