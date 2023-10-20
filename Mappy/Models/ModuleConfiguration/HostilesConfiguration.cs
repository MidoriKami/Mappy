using System.Drawing;
using System.Numerics;
using Dalamud.Interface;

namespace Mappy.Models.ModuleConfiguration; 

public class HostilesConfiguration : IModuleConfig, IIconConfig, ITooltipConfig {
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 10;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.75f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.White.Vector();
}