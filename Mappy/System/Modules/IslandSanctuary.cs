using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.MJI;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;

namespace Mappy.System.Modules;

[Category("ModuleConfig")]
public class IslandSanctuaryConfig : IModuleConfig, IIconConfig, ITooltipConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 5;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.LightBlue.AsVector4();

    [BoolConfig("Logging")]
    public bool Logging { get; set; } = true;
    
    [BoolConfig("Harvesting")]
    public bool Harvesting { get; set; } = true;
    
    [BoolConfig("Quarrying")]
    public bool Quarrying { get; set; } = true;
    
    [BoolConfig("Mining")]
    public bool Mining { get; set; } = true;
    
    [BoolConfig("Fishing")]
    public bool Fishing { get; set; } = true;
}

public unsafe class IslandSanctuary : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.IslandSanctuary;
    public override IModuleConfig Configuration { get; protected set; } = new IslandSanctuaryConfig();

    private readonly List<MJIGatheringObject> gatheringObjectNames = LuminaCache<MJIGatheringObject>.Instance
        .Where(obj => obj is not { RowId: 0 })
        .ToList();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (MJIManager.Instance()->IsPlayerInSanctuary is 0) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    public override void LoadForMap(MapData mapData) { }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        DrawGatheringIcons(viewport, map);

        DrawUtilities.DrawLevelObjective(LuminaCache<Level>.Instance.GetRow(10011038), 60987, "TEST", KnownColor.Red.AsVector4(), KnownColor.White.AsVector4(), viewport, map, true);
    }
    
    private void DrawGatheringIcons(Viewport viewport, Map map)
    {
        if (MJIManager.Instance()->CurrentMode is not 1) return; // If not in GatherMode
        var config = GetConfig<IslandSanctuaryConfig>();
        
        
        foreach (var obj in Service.ObjectTable)
        {
            if(obj.ObjectKind is not ObjectKind.CardStand) continue;
            if (gatheringObjectNames.FirstOrDefault(luminaObject => string.Compare(luminaObject.Name.Value?.Singular.RawString,obj.Name.TextValue, StringComparison.OrdinalIgnoreCase) == 0) is not { MapIcon: var mapIcon } luminaData ) continue;

            switch (luminaData.Unknown2)
            {
                case 1 when !config.Logging:
                case 2 when !config.Harvesting:
                case 4 when !config.Quarrying:
                case 8 when !config.Mining:
                case 16 when !config.Fishing:
                    continue;
            }
            
            if(config.ShowIcon) DrawUtilities.DrawIcon(mapIcon,obj, map, config.IconScale);
            if(config.ShowTooltip) DrawUtilities.DrawTooltip(obj.Name.TextValue, config.TooltipColor, mapIcon);
        }

    }
}