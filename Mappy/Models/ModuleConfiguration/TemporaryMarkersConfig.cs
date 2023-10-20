using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using KamiLib.AutomaticUserInterface;

namespace Mappy.Models.ModuleConfiguration; 

[Category("ModuleColors")]
public class TemporaryMarkersConfig : IModuleConfig, IIconConfig, ITooltipConfig {
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 13;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.LightSkyBlue.Vector();
    
    [ColorConfig("CircleColor", 65, 105, 225, 45)]
    public Vector4 CircleColor { get; set; } = KnownColor.RoyalBlue.Vector() with { W = 0.33f };
}