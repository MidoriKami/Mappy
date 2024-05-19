using System.Drawing;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;

namespace Mappy.Classes;

public partial class MapRenderer {
    private unsafe void DrawGameObjects() {
        if (Service.ClientState is { LocalPlayer: { } player }) {
            var position = ImGui.GetWindowPos() +
                           DrawPosition +
                           (new Vector2(player.Position.X, player.Position.Z) * AgentMap.Instance()->SelectedMapSizeFactorFloat +
                            new Vector2(AgentMap.Instance()->SelectedOffsetX, AgentMap.Instance()->SelectedOffsetY) +
                            new Vector2(1024.0f, 1024.0f)) *
                           Scale;

            ImGui.GetWindowDrawList().AddCircleFilled(position, 150.0f * Scale, ImGui.GetColorU32(KnownColor.Gray.Vector() with { W = 0.10f }));
            ImGui.GetWindowDrawList().AddCircle(position, 150.0f * Scale, ImGui.GetColorU32(KnownColor.Gray.Vector() with { W = 0.30f }));
        }

        foreach (var obj in Service.ObjectTable) {
            if (!obj.IsTargetable) continue;

            DrawHelpers.DrawMapMarker(new MarkerInfo {
                Position = (new Vector2(obj.Position.X, obj.Position.Z) * AgentMap.Instance()->SelectedMapSizeFactorFloat + new Vector2(AgentMap.Instance()->SelectedOffsetX, AgentMap.Instance()->SelectedOffsetY) + new Vector2(1024.0f, 1024.0f)) *
                           Scale,
                Offset = DrawPosition,
                Scale = Scale,
                IconId = obj.ObjectKind switch {
                    ObjectKind.Player => 60421,
                    ObjectKind.BattleNpc when obj is { SubKind: (int) BattleNpcSubKind.Enemy, TargetObject: not null } => 60422,
                    ObjectKind.BattleNpc when obj is { SubKind: (int) BattleNpcSubKind.Enemy, TargetObject: null } => 60424,
                    ObjectKind.BattleNpc when obj.SubKind == (int) BattleNpcSubKind.Pet => 60961,
                    ObjectKind.Treasure => 60003,
                    ObjectKind.GatheringPoint => System.GatheringPointIconCache.GetValue(obj.DataId),
                    _ => 0,
                },

                PrimaryText = () => GetTooltipForGameObject(obj),
            });
        }
    }

    private string GetTooltipForGameObject(GameObject obj)
        => obj switch {
            BattleNpc battleNpc => $"Lv. {battleNpc.Level} {battleNpc.Name}",
            PlayerCharacter playerCharacter => $"Lv. {playerCharacter.Level} {playerCharacter.Name}",
            _ => obj.ObjectKind switch {
                ObjectKind.GatheringPoint => System.GatheringPointNameCache.GetValue((obj.DataId, obj.Name.ToString())) ?? string.Empty,
                ObjectKind.Treasure => obj.Name.ToString(),
                _ => string.Empty,
            }
        };
}