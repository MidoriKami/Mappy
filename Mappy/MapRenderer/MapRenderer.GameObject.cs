using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using Mappy.Classes;
using Mappy.Extensions;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    private unsafe void DrawGameObjects() {
        if (AgentMap.Instance()->SelectedMapId != AgentMap.Instance()->CurrentMapId) return;
        
        if (Service.ClientState is not { LocalPlayer: { } player }) return;

        if (System.SystemConfig.ShowRadar) {
            DrawRadar(player);
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
                    ObjectKind.Player => 60421,
                    ObjectKind.BattleNpc when obj is { SubKind: (int) BattleNpcSubKind.Enemy, TargetObject: not null } => 60422,
                    ObjectKind.BattleNpc when obj is { SubKind: (int) BattleNpcSubKind.Enemy, TargetObject: null } => 60424,
                    ObjectKind.BattleNpc when obj.SubKind == (int) BattleNpcSubKind.Pet => 60961,
                    ObjectKind.Treasure when Vector3.Distance(obj.Position, player.Position) <= 50.0f => 60003,
                    ObjectKind.GatheringPoint => System.GatheringPointIconCache.GetValue(obj.DataId),
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
        
        ImGui.GetWindowDrawList().AddCircleFilled(position, 150.0f * Scale, ImGui.GetColorU32(KnownColor.Gray.Vector() with { W = 0.10f }));
        ImGui.GetWindowDrawList().AddCircle(position, 150.0f * Scale, ImGui.GetColorU32(KnownColor.Gray.Vector() with { W = 0.30f }));
    }

    private string GetTooltipForGameObject(IGameObject obj) {
        if (Service.PluginInterface.TryGetData<Dictionary<ulong, string>>("PetRenamer.GameObjectRenameDict", out var dictionary)) {
            if (dictionary.TryGetValue(obj.GameObjectId, out var newName)) {
                return newName;
            }
        }
        
        return obj switch {
            IBattleNpc { Level: > 0 } battleNpc => $"Lv. {battleNpc.Level} {battleNpc.Name}",
            IPlayerCharacter { Level: > 0 } playerCharacter => $"Lv. {playerCharacter.Level} {playerCharacter.Name}",
            _ => obj.ObjectKind switch {
                ObjectKind.GatheringPoint => System.GatheringPointNameCache.GetValue((obj.DataId, obj.Name.ToString())) ?? string.Empty,
                ObjectKind.Treasure => obj.Name.ToString(),
                _ => string.Empty,
            },
        };
    }
}