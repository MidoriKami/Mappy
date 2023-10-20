using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using KamiLib.AutomaticUserInterface;
using Mappy.Views.Attributes;

namespace Mappy.Models.ModuleConfiguration; 

[Category("IconSelection")]
public class TreasureConfig : IModuleConfig, IIconConfig, ITooltipConfig {
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 4;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.White.Vector();
    
    [IconSelection(60003, 60354)]
    public uint SelectedIcon { get; set; } = 60003;
}