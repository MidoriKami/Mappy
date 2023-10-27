using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.MJI;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;

namespace Mappy.System.Modules;

public unsafe class IslandSanctuary : ModuleBase {
    public override ModuleName ModuleName => ModuleName.IslandSanctuary;
    public override IModuleConfig Configuration { get; protected set; } = new IslandSanctuaryConfig();

    private readonly List<MJIGatheringObject> gatheringObjectNames = LuminaCache<MJIGatheringObject>.Instance
        .Where(obj => obj is not { RowId: 0 })
        .ToList();

    protected override bool ShouldDrawMarkers(Map map) {
        if (MJIManager.Instance()->IsPlayerInSanctuary is 0) return false;
        if (LuminaCache<TerritoryType>.Instance.GetRow(map.TerritoryType.Row) is not { TerritoryIntendedUse: 49 }) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    protected override void UpdateMarkers(Viewport viewport, Map map) {
        if (MJIManager.Instance()->CurrentMode is not 1) return; // If not in GatherMode
        var config = GetConfig<IslandSanctuaryConfig>();

        foreach (var obj in Service.ObjectTable) {
            if (obj.ObjectKind is not ObjectKind.CardStand) continue;
            if (gatheringObjectNames.FirstOrDefault(luminaObject => string.Equals(luminaObject.Name.Value?.Singular.RawString, obj.Name.TextValue, StringComparison.OrdinalIgnoreCase)) is not { MapIcon: var mapIcon } luminaData) continue;

            switch (luminaData.Unknown2) {
                case 1 when !config.Logging:
                case 2 when !config.Harvesting:
                case 4 when !config.Quarrying:
                case 8 when !config.Mining:
                case 16 when !config.Fishing:
                    continue;
            }

            UpdateIcon(obj.Position, () => new MappyMapIcon {
                MarkerId = obj.Position,
                IconId = mapIcon,
                ObjectPosition = new Vector2(obj.Position.X, obj.Position.Z),
                Tooltip = obj.Name.TextValue,
            });
        }
    }
}