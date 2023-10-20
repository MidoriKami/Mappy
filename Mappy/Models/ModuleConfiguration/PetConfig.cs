using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using KamiLib.AutomaticUserInterface;
using Mappy.Views.Attributes;

namespace Mappy.Models.ModuleConfiguration; 

[Category("IconSelection")]
public class PetConfig : IModuleConfig, IIconConfig, ITooltipConfig {
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 5;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.75f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.MediumPurple.Vector();
    
    [IconSelection(60961)]
    public uint SelectedIcon { get; set; } = 60961;
}