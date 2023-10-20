using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using KamiLib.AutomaticUserInterface;

namespace Mappy.Models.ModuleConfiguration; 

[Category("ModuleConfig")]
public class IslandSanctuaryConfig : IModuleConfig, IIconConfig, ITooltipConfig {
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 5;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.LightBlue.Vector();

    [BoolConfig("Logging")]
    public bool Logging { get; set; } = true;
    
    [BoolConfig("Harvesting")]
    public bool Harvesting { get; set; } = true;
    
    [BoolConfig("Quarrying")]
    public bool Quarrying { get; set; } = true;
    
    [BoolConfig("Mining")]
    public bool Mining { get; set; } = true;
    
    [BoolConfig("Fishing")]
    public bool Fishing { get; set; } = true;
}