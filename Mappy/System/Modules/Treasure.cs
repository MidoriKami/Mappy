using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using Mappy.Views.Attributes;

namespace Mappy.System.Modules;

[Category("IconSelection")]
public class TreasureConfig : IModuleConfig, IIconConfig, ITooltipConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 4;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.White.AsVector4();
    
    [IconSelection(60003, 60354)]
    public uint SelectedIcon { get; set; } = 60003;
}

public class Treasure : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.TreasureMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new TreasureConfig();

    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<TreasureConfig>();
        
        foreach (var obj in Service.ObjectTable)
        {
            if (obj.ObjectKind != ObjectKind.Treasure) continue;
            if(!IsTargetable(obj)) continue;
            
            DrawUtilities.DrawMapIcon(new MappyMapIcon
            {
                IconId = config.SelectedIcon,
                ObjectPosition = new Vector2(obj.Position.X, obj.Position.Z),

                Tooltip = obj.Name.TextValue,
            }, config, viewport, map);
        }
    }

    private static bool IsTargetable(GameObject gameObject)
    {
        if (gameObject.Address == nint.Zero) return false;

        if (Service.ClientState.LocalPlayer is not { Position: var playerPosition } ) return false;

        // Limit height delta to 15yalms
        return Math.Abs(playerPosition.Y - gameObject.Position.Y) < 15.0f && gameObject.IsTargetable;
    }
}