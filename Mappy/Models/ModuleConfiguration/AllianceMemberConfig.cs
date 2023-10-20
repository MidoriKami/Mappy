using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using KamiLib.AutomaticUserInterface;
using Mappy.Views.Attributes;

namespace Mappy.Models.ModuleConfiguration; 

[Category("IconSelection", 1)]
public interface IAllianceMemberIconSelection {
    [IconSelection(60358, 60359, 60360, 60361)]
    public uint SelectedIcon { get; set; }
}

[Category("ModuleConfig")]
public class AllianceMemberConfig : IModuleConfig, IIconConfig, ITooltipConfig, IAllianceMemberIconSelection {
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 7;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.LightGreen.Vector();
    
    public uint SelectedIcon { get; set; } = 60358;
    
    [BoolConfig("DisplayJobIcons", "DisplayJobIconsHelp")]
    public bool DisplayJobIcons { get; set; } = false;
}