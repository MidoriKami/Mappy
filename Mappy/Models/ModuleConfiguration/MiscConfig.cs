using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using KamiLib.AutomaticUserInterface;

namespace Mappy.Models.ModuleConfiguration; 

[Category("ModuleConfig")]
public class MiscConfig : IModuleConfig, IIconConfig, ITooltipConfig {
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 11;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.White.Vector();
}