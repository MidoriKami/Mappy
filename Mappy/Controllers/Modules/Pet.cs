using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;

namespace Mappy.System.Modules;

public class Pet : ModuleBase {
    public override ModuleName ModuleName => ModuleName.Pets;
    public override IModuleConfig Configuration { get; protected set; } = new PetConfig();

    protected override bool ShouldDrawMarkers(Map map) {
        if (!IsPlayerInCurrentMap(map)) return false;

        return base.ShouldDrawMarkers(map);
    }
    
    protected override void UpdateMarkers(Viewport viewport, Map map) {
        foreach (var obj in Service.PartyList) {
            DrawPet(obj.ObjectId);
        }

        if (Service.PartyList.Length is 0 && Service.ClientState.LocalPlayer is { } player) {
            DrawPet(player.ObjectId);
        }
    }
    
    private void DrawPet(uint ownerID) {
        var config = GetConfig<PetConfig>();
        
        foreach (var obj in OwnedPets(ownerID)) {
            UpdateIcon(obj.ObjectId, () => new MappyMapIcon {
                MarkerId = obj.ObjectId,
                IconId = config.SelectedIcon,
                ObjectPosition = new Vector2(obj.Position.X, obj.Position.Z),
                Tooltip = obj.Name.TextValue,
            }, icon => {
                icon.IconId = config.SelectedIcon;
                icon.ObjectPosition = new Vector2(obj.Position.X, obj.Position.Z);
            });
        }
    }
    
    private static IEnumerable<GameObject> OwnedPets(uint objectID) 
        => Service.ObjectTable
            .Where(obj => obj.OwnerId == objectID)
            .Where(obj => obj.ObjectKind == ObjectKind.BattleNpc && IsPetOrChocobo(obj));

    private static bool IsPetOrChocobo(GameObject gameObject) => (BattleNpcSubKind?) (gameObject as BattleNpc)?.SubKind switch {
        BattleNpcSubKind.Chocobo => true,
        BattleNpcSubKind.Enemy => false,
        BattleNpcSubKind.None => false,
        BattleNpcSubKind.Pet => true,
        _ => false
    };
}