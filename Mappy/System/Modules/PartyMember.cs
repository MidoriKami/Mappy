using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using KamiLib.AutomaticUserInterface;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using Mappy.Views.Attributes;
using ClientStructPartyMember = FFXIVClientStructs.FFXIV.Client.Game.Group.PartyMember;

namespace Mappy.System.Modules;

[Category("IconSelection", 1)]
public interface IPartyMemberIconSelection
{
    [IconSelection(60421, 63940, 63944, 63937, 63946)]
    public uint SelectedIcon { get; set; }
}

[Category("ModuleConfig")]
public class PartyMemberConfig : IModuleConfig, IIconConfig, ITooltipConfig, IPartyMemberIconSelection
{
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

public unsafe class PartyMember : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.PartyMembers;
    public override IModuleConfig Configuration { get; protected set; } = new PartyMemberConfig();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!IsPlayerInCurrentMap(map)) return false;
        
        return base.ShouldDrawMarkers(map);
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        if (Service.ClientState.LocalPlayer is not { ObjectId: var playerObjectId }) return;
        var config = GetConfig<PartyMemberConfig>();

        foreach (var member in AdjustedPartyMemberSpan)
        {
            if (member.ObjectID is 0xE0000000 or 0) continue;
            if (member.ObjectID == playerObjectId) continue;
            
            DrawUtilities.DrawMapIcon(new MappyMapIcon
            {
                IconId = config.DisplayJobIcons ? member.ClassJob + 62000u : config.SelectedIcon,
                ObjectPosition = new Vector2(member.X, member.Z),
                
                TooltipExtraIcon = member.ClassJob + 62000u,
                Tooltip = MemoryHelper.ReadStringNullTerminated((nint)member.Name),
            }, config, viewport, map);
        }
    }

    private Span<ClientStructPartyMember> AdjustedPartyMemberSpan => new(GroupManager.Instance()->PartyMembers, GroupManager.Instance()->MemberCount);
}