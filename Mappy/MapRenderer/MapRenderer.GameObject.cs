using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using KamiLib.Extensions;
using Lumina.Excel.Sheets;
using Mappy.Classes;
using Mappy.Extensions;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    private unsafe void DrawGameObjects() {
        if (AgentMap.Instance()->SelectedMapId != AgentMap.Instance()->CurrentMapId) return;
        
        if (Service.ClientState is not { LocalPlayer: { } player }) return;

        if (System.SystemConfig.ShowRadar) {
            if ((Service.Condition.IsBoundByDuty() && System.SystemConfig.ShowRadarInDuties) || !Service.Condition.IsBoundByDuty()) {
                DrawRadar(player);
            }
        }

        foreach (var obj in Service.ObjectTable.Reverse()) {
            if (!obj.IsTargetable) continue;
            if (GroupManager.Instance()->MainGroup.IsEntityIdInParty(obj.EntityId)) continue;
            if (GroupManager.Instance()->MainGroup.IsEntityIdInAlliance(obj.EntityId)) continue;
            if (Vector3.Distance(obj.Position, player.Position) >= 150.0f) continue;

            DrawHelpers.DrawMapMarker(new MarkerInfo {
                Position = (obj.GetMapPosition() -
                            DrawHelpers.GetMapOffsetVector() +
                            DrawHelpers.GetMapCenterOffsetVector()) * Scale,
                Offset = DrawPosition,
                Scale = Scale,
                IconId = obj.ObjectKind switch {
                    ObjectKind.Player when GroupManager.Instance()->MainGroup.MemberCount is 0 && System.SystemConfig.ShowPlayers => 60421,
                    ObjectKind.Player when System.SystemConfig.ShowPlayers => 60444,
                    ObjectKind.BattleNpc when IsBoss(obj) && obj.TargetObject is null => 60402,
                    ObjectKind.BattleNpc when IsBoss(obj) && obj.TargetObject is not null => 60401,
                    ObjectKind.BattleNpc when obj is { SubKind: (int) BattleNpcSubKind.Enemy, TargetObject: not null } => 60422,
                    ObjectKind.BattleNpc when obj is { SubKind: (int) BattleNpcSubKind.Enemy, TargetObject: null } => 60424,
                    ObjectKind.BattleNpc when obj.SubKind == (int) BattleNpcSubKind.Pet => 60961,
                    ObjectKind.Treasure => 60003,
                    ObjectKind.GatheringPoint => System.GatheringPointIconCache.GetValue(obj.DataId),
                    ObjectKind.EventObj when IsAetherCurrent(obj) => 60653,
                    _ => 0,
                },

                PrimaryText = () => GetTooltipForGameObject(obj),
            });
        }
    }

    private void DrawRadar(IPlayerCharacter gameObjectCenter) {
        var position = ImGui.GetWindowPos() +
                       DrawPosition +
                       (gameObjectCenter.GetMapPosition() -
                        DrawHelpers.GetMapOffsetVector() +
                        DrawHelpers.GetMapCenterOffsetVector()) * Scale;
        
        ImGui.GetWindowDrawList().AddCircleFilled(position, 150.0f * Scale, ImGui.GetColorU32(System.SystemConfig.RadarColor));
        ImGui.GetWindowDrawList().AddCircle(position, 150.0f * Scale, ImGui.GetColorU32(System.SystemConfig.RadarOutlineColor));
    }

    private string GetTooltipForGameObject(IGameObject obj) {
        return obj switch {
            IBattleNpc { Level: > 0 } battleNpc => $"Lv. {battleNpc.Level} {battleNpc.Name}",
            IPlayerCharacter { Level: > 0 } playerCharacter => $"Lv. {playerCharacter.Level} {playerCharacter.Name}",
            _ => obj.ObjectKind switch {
                ObjectKind.GatheringPoint => System.GatheringPointNameCache.GetValue((obj.DataId, obj.Name.ToString())) ?? string.Empty,
                ObjectKind.Treasure => obj.Name.ToString(),
                ObjectKind.EventObj when IsAetherCurrent(obj) => obj.Name.ToString(),
                _ => string.Empty,
            },
        };
    }

    private unsafe bool IsAetherCurrent(IGameObject gameObject) {
        if (gameObject.ObjectKind is not ObjectKind.EventObj) return false;

        var csEventObject = (GameObject*) gameObject.Address;
        if (csEventObject is null) return false;

        if (csEventObject->EventHandler is null) return false;
        return csEventObject->EventHandler->Info.EventId.ContentId == EventHandlerContent.AetherCurrent;
    }

    private bool IsBoss(IGameObject chara)
        => Service.DataManager.GetExcelSheet<BNpcBase>().GetRow(chara.DataId).Rank is 2 or 6;
}
