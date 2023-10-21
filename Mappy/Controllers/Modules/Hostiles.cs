using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;

namespace Mappy.System.Modules;

public class Hostiles : ModuleBase {
    public override ModuleName ModuleName => ModuleName.Hostiles;
    public override IModuleConfig Configuration { get; protected set; } = new HostilesConfiguration();

    private const uint YellowEnemyIcon = 60424u;
    private const uint RedEnemyIcon = 60422u;

    protected override bool ShouldDrawMarkers(Map map) {
        if (!IsPlayerInCurrentTerritory(map)) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    protected override void UpdateMarkers(Viewport viewport, Map map) {
        foreach (var obj in Service.ObjectTable) {
            if (obj as BattleNpc is not { BattleNpcKind: BattleNpcSubKind.Enemy } battleNpc) continue;
            
            UpdateIcon(obj.ObjectId, () => new MappyMapIcon {
                MarkerId = obj.ObjectId,
                IconId = GetIconForBattleNpc(battleNpc),
                ObjectPosition = new Vector2(battleNpc.Position.X, battleNpc.Position.Z),
                Tooltip = $"Lv. {battleNpc.Level} {battleNpc.Name.TextValue}",
            }, icon => {
                icon.IconId = GetIconForBattleNpc(battleNpc);
                icon.ObjectPosition = new Vector2(battleNpc.Position.X, battleNpc.Position.Z);
            });
        }
    }

    private static uint GetIconForBattleNpc(GameObject battleNpc) {
        if (battleNpc is { IsTargetable: false } or { IsDead: true }) return 0;
        
        return battleNpc.TargetObject is not null ? RedEnemyIcon : YellowEnemyIcon;
    }
}