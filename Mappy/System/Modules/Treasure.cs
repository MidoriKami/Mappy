using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
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
        foreach (var obj in Service.ObjectTable)
        {
            if(obj.ObjectKind != ObjectKind.Treasure) continue;
            var config = GetConfig<TreasureConfig>();

            if(!IsTargetable(obj)) continue;
            
            if(config.ShowIcon) DrawUtilities.DrawGameObjectIcon(config.SelectedIcon, obj, map, config.IconScale);
            if(config.ShowTooltip) DrawTooltip(obj);
        }
    }
    
    private void DrawTooltip(GameObject gameObject)
    {
        if (!ImGui.IsItemHovered()) return;
        var config = GetConfig<TreasureConfig>();
        
        if (gameObject.Name.TextValue is { Length: > 0 } name)
        {
            DrawUtilities.DrawTooltip(config.SelectedIcon, config.TooltipColor, name);
        }
    }

    private static bool IsTargetable(GameObject gameObject)
    {
        if (gameObject.Address == nint.Zero) return false;

        if (Service.ClientState.LocalPlayer is not { } player) return false;

        // Limit height delta to 20yalms
        if (Math.Abs(player.Position.Y - gameObject.Position.Y) < 15.0f)
        {
            return gameObject.IsTargetable;
        }

        return false;
    }
}