using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Classes;

namespace Mappy.MapRenderer;

public unsafe partial class MapRenderer
{
    private void DrawGroupMembers()
    {
        foreach (var partyMember in GroupManager.Instance()->MainGroup.PartyMembers[..GroupManager.Instance()->MainGroup.MemberCount]) {
            if (partyMember.EntityId is 0xE0000000) continue;
            if (partyMember.TerritoryType != AgentMap.Instance()->SelectedTerritoryId) continue;

            DrawHelpers.DrawMapMarker(new MarkerInfo
            {
                Position = (new Vector2(partyMember.Position.X, partyMember.Position.Z) * DrawHelpers.GetMapScaleFactor() -
                            DrawHelpers.GetMapOffsetVector() +
                            DrawHelpers.GetMapCenterOffsetVector()) * Scale,
                Offset = DrawPosition,
                Scale = Scale,
                IconId = 60421,
                PrimaryText = () => $"Lv. {partyMember.Level} {partyMember.NameString}",
            });
        }

        foreach (var allianceMember in GroupManager.Instance()->MainGroup.AllianceMembers) {
            if (allianceMember.EntityId is 0xE0000000) continue;
            if (AgentMap.Instance()->SelectedMapId != AgentMap.Instance()->CurrentMapId) continue;

            DrawHelpers.DrawMapMarker(new MarkerInfo
            {
                Position = (new Vector2(allianceMember.Position.X, allianceMember.Position.Z) * DrawHelpers.GetMapScaleFactor() -
                            DrawHelpers.GetMapOffsetVector() +
                            DrawHelpers.GetMapCenterOffsetVector()) * Scale,
                Offset = DrawPosition,
                Scale = Scale,
                IconId = 60403,
                PrimaryText = () => $"Lv. {allianceMember.Level} {allianceMember.NameString}",
            });
        }
    }
}