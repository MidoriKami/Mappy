using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using KamiLib.AutomaticUserInterface;

namespace Mappy.Models.ModuleConfiguration; 

[Category("ModuleColors")]
public interface IMapIconColorConfig {
    [ColorConfig("MapLinkColor", 0.655f, 0.396f, 0.149f, 1.0f)]
    public Vector4 MapLinkColor { get; set; } 
    
    [ColorConfig("InstanceLinkColor", 255, 165, 0, 255)]
    public Vector4 InstanceLinkColor { get; set; } 
    
    [ColorConfig("AetheryteColor", 65, 105, 225, 255)]
    public Vector4 AetheryteColor { get; set; }
    
    [ColorConfig("AethernetColor", 173, 216, 230, 255)]
    public Vector4 AethernetColor { get; set; } 
}

[Category("ModuleConfig")]
public class MapIconConfig : IModuleConfig, IIconConfig, ITooltipConfig, IMapIconColorConfig {
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 1;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    
    public Vector4 TooltipColor { get; set; } = KnownColor.White.Vector();
    public Vector4 MapLinkColor { get; set; } = new(0.655f, 0.396f, 0.149f, 1.0f);
    public Vector4 InstanceLinkColor { get; set; } = KnownColor.Orange.Vector();
    public Vector4 AetheryteColor { get; set; } = KnownColor.RoyalBlue.Vector();
    public Vector4 AethernetColor { get; set; }  = KnownColor.LightBlue.Vector();
    
    [BoolConfig("AetherytesOnTop")]
    public bool AetherytesOnTop { get; set; } = true;
    
    [BoolConfig("ShowMiscTooltips", "ShowMiscTooltipsHelp")]
    public bool ShowMiscTooltips { get; set; } = true;

    [BoolConfig("ShowTeleportCosts")]
    public bool ShowTeleportCostTooltips { get; set; } = true;

    [BoolConfig("ShowSubzoneLabels")]
    public bool ShowSubzoneLabels { get; set; } = true;
}
