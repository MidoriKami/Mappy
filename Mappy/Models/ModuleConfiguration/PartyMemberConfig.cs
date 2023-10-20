using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using KamiLib.AutomaticUserInterface;
using Mappy.Views.Attributes;

namespace Mappy.Models.ModuleConfiguration; 

[Category("IconSelection", 1)]
public interface IPartyMemberIconSelection {
    [IconSelection(60421, 63940, 63944, 63937, 63946)]
    public uint SelectedIcon { get; set; }
}

[Category("ModuleConfig")]
public class PartyMemberConfig : IModuleConfig, IIconConfig, ITooltipConfig, IPartyMemberIconSelection {
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 7;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.DodgerBlue.Vector();

    public uint SelectedIcon { get; set; } = 60421;
    
    [BoolConfig("DisplayJobIcons", "DisplayJobIconsHelp")]
    public bool DisplayJobIcons { get; set; } = false;
}