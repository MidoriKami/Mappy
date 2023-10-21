using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;
using Mappy.Utility;

namespace Mappy.System.Modules;

public unsafe class AllianceMember : ModuleBase {
    public override ModuleName ModuleName => ModuleName.AllianceMembers;
    public override IModuleConfig Configuration { get; protected set; } = new AllianceMemberConfig();

    protected override bool ShouldDrawMarkers(Map map) {
        if (!DutyLists.Instance.IsType(Service.ClientState.TerritoryType, DutyType.Alliance)) return false;
        
        return base.ShouldDrawMarkers(map);
    }
    protected override void UpdateMarkers(Viewport viewport, Map map) {
        var config = GetConfig<AllianceMemberConfig>();
        
        foreach (var member in GroupManager.Instance()->AllianceMembersSpan) {
            if (member.ObjectID is 0xE0000000 or 0) continue;

            UpdateIcon(member.ObjectID, () => new MappyMapIcon {
                MarkerId = member.ObjectID,
                IconId = config.DisplayJobIcons ? member.ClassJob + 62000u : config.SelectedIcon,
                ObjectPosition = new Vector2(member.X, member.Z),
                TooltipExtraIcon = member.ClassJob + 62000u,
                Tooltip = member.GetName(),
            }, icon => {
                icon.IconId = config.DisplayJobIcons ? member.ClassJob + 62000u : config.SelectedIcon;
                icon.ObjectPosition = new Vector2(member.X, member.Z);
                icon.TooltipExtraIcon = member.ClassJob + 62000u;
            });
        }
    }
}