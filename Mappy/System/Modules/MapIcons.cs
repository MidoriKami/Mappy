using System.Collections.Generic;
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

public class MapIconConfig : IconModuleConfigBase
{
    [Disabled] // Don't allow disabling all icons.
    public new bool ShowIcon = true;
    
    [BoolConfigOption("AetherytesOnTop", "ModuleConfig", 0)]
    public bool AetherytesOnTop = true;
    
    [ColorConfigOption("StandardColor", "ModuleColors", 1, 255, 255, 255, 255)]
    public Vector4 StandardColor = KnownColor.White.AsVector4();
    
    [ColorConfigOption("MapLinkColor", "ModuleColors", 1, 0.655f, 0.396f, 0.149f, 1.0f)]
    public Vector4 MapLinkColor = new(x: 0.655f, 0.396f, 0.149f, 1.0f);
    
    [ColorConfigOption("InstanceLinkColor", "ModuleColors", 1, 255, 165, 0, 255)]
    public Vector4 InstanceLinkColor = KnownColor.Orange.AsVector4();
    
    [ColorConfigOption("AetheryteColor", "ModuleColors", 1, 0, 0, 255, 255)]
    public Vector4 AetheryteColor = KnownColor.Blue.AsVector4();
    
    [ColorConfigOption("AethernetColor", "ModuleColors", 1, 173, 216, 230, 255)]
    public Vector4 AethernetColor = KnownColor.LightBlue.AsVector4();
    
    [MapMarkerSelection(null, "DisplayedMarkers", 3)]
    public HashSet<uint> DisabledMarkers = new();
}

public class MapIcons : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.MapMarkers;
    public override ModuleConfigBase Configuration { get; protected set; } = new MapIconConfig();

    public override void LoadForMap(MapData mapData)
    {
        
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        
    }
}