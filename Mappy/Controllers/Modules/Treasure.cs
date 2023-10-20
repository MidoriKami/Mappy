using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;
using Mappy.Utility;

namespace Mappy.System.Modules;

public class Treasure : ModuleBase {
    public override ModuleName ModuleName => ModuleName.TreasureMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new TreasureConfig();

    protected override void DrawMarkers(Viewport viewport, Map map) {
        var config = GetConfig<TreasureConfig>();
        
        foreach (var obj in Service.ObjectTable) {
            if (obj.ObjectKind != ObjectKind.Treasure) continue;
            if(!IsTargetable(obj)) continue;
            
            DrawUtilities.DrawMapIcon(new MappyMapIcon {
                IconId = config.SelectedIcon,
                ObjectPosition = new Vector2(obj.Position.X, obj.Position.Z),

                Tooltip = obj.Name.TextValue,
            }, config, viewport, map);
        }
    }

    private static bool IsTargetable(GameObject gameObject) {
        if (gameObject.Address == nint.Zero) return false;

        if (Service.ClientState.LocalPlayer is not { Position: var playerPosition } ) return false;

        // Limit height delta to 15yalms
        return Math.Abs(playerPosition.Y - gameObject.Position.Y) < 15.0f && gameObject.IsTargetable;
    }
}