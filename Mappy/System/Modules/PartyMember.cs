using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using Mappy.Views.Attributes;
using ClientStructPartyMember = FFXIVClientStructs.FFXIV.Client.Game.Group.PartyMember;

namespace Mappy.System.Modules;

public class PartyMemberConfig : IconModuleConfigBase
{
    [ColorConfigOption("TooltipColor", "ModuleColors", 1, 65, 105, 225, 255)]
    public Vector4 TooltipColor = KnownColor.RoyalBlue.AsVector4();

    [IconSelection("SelectedIcon", "IconSelection", 2, 60421)]
    public uint SelectedIcon = 60421;
}

public unsafe class PartyMember : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.PartyMembers;
    public override ModuleConfigBase Configuration { get; protected set; } = new PartyMemberConfig();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!IsPlayerInCurrentMap(map)) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    public override void LoadForMap(MapData mapData)
    {
        // Do Nothing.
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<PartyMemberConfig>();
        
        foreach (var member in AdjustedPartyMemberSpan)
        {
            if (member.ObjectID is 0xE0000000 or 0) continue;
            
            var memberPosition = new Vector2(member.X, member.Z);
            var objectPosition = Position.GetObjectPosition(memberPosition, map);
            
            if(config.ShowIcon) DrawUtilities.DrawIcon(config.SelectedIcon, objectPosition, config.IconScale);
            if(config.ShowTooltip) DrawUtilities.DrawTooltip(MemoryHelper.ReadStringNullTerminated((nint)member.Name), config.TooltipColor);
        }
    }

    private Span<ClientStructPartyMember> AdjustedPartyMemberSpan => new(GroupManager.Instance()->PartyMembers, GroupManager.Instance()->MemberCount);
}