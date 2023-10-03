using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using KamiLib.AutomaticUserInterface;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using Mappy.Views.Attributes;

namespace Mappy.System.Modules;

[Category("IconSelection", 1)]
public interface IAllianceMemberIconSelection
{
    [IconSelection(60358, 60359, 60360, 60361)]
    public uint SelectedIcon { get; set; }
}

[Category("ModuleConfig")]
public class AllianceMemberConfig : IModuleConfig, IIconConfig, ITooltipConfig, IAllianceMemberIconSelection
{
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

public unsafe class AllianceMember : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.AllianceMembers;
    public override IModuleConfig Configuration { get; protected set; } = new AllianceMemberConfig();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!DutyLists.Instance.IsType(Service.ClientState.TerritoryType, DutyType.Alliance)) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<AllianceMemberConfig>();
        
        foreach (var member in GroupManager.Instance()->AllianceMembersSpan)
        {
            if (member.ObjectID is 0xE0000000 or 0) continue;
            
            DrawUtilities.DrawMapIcon(new MappyMapIcon
            {
                IconId = config.DisplayJobIcons ? member.ClassJob + 62000u : config.SelectedIcon,
                ObjectPosition = new Vector2(member.X, member.Z),
                
                TooltipExtraIcon = member.ClassJob + 62000u,
                Tooltip = MemoryHelper.ReadStringNullTerminated((nint)member.Name),
                
            }, config, viewport, map);
        }
    }
}