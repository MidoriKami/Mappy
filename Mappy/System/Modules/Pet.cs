using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using KamiLib.AutomaticUserInterface;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using Mappy.Views.Attributes;

namespace Mappy.System.Modules;

public class PetConfig : IconModuleConfigBase
{
    [ColorConfigOption("TooltipColor", "ModuleColors", 1, 0.765f, 0.260f, 0.765f, 1.00f)]
    public Vector4 TooltipColor = new(0.765f, 0.260f, 0.765f, 1.00f);

    [IconSelection("SelectedIcon", "IconSelection", 1, 60961)]
    public uint SelectedIcon = 60961;
}

public class Pet : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.Pets;
    public override ModuleConfigBase Configuration { get; protected set; } = new PetConfig();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!IsPlayerInCurrentMap(map)) return false;

        return base.ShouldDrawMarkers(map);
    }

    public override void LoadForMap(MapData mapData)
    {
        // Do Nothing.
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        foreach (var obj in Service.PartyList)
        {
            DrawPet(obj.ObjectId, map);
        }

        if (Service.PartyList.Length is 0 && Service.ClientState.LocalPlayer is { } player)
        {
            DrawPet(player.ObjectId, map);
        }
    }
    
    private void DrawPet(uint ownerID, Map map)
    {
        var config = GetConfig<PetConfig>();
        
        foreach (var obj in OwnedPets(ownerID))
        {
            if(config.ShowIcon) DrawUtilities.DrawIcon(config.SelectedIcon, obj, map, config.IconScale + 0.25f);
            if(config.ShowTooltip) DrawUtilities.DrawTooltip(obj.Name.TextValue, config.TooltipColor);
        }
    }
    
    private static IEnumerable<GameObject> OwnedPets(uint objectID) => Service.ObjectTable
        .Where(obj => obj.OwnerId == objectID)
        .Where(obj => obj.ObjectKind == ObjectKind.BattleNpc && IsPetOrChocobo(obj));

    private static bool IsPetOrChocobo(GameObject gameObject) => (BattleNpcSubKind?) (gameObject as BattleNpc)?.SubKind switch
    {
        BattleNpcSubKind.Chocobo => true,
        BattleNpcSubKind.Enemy => false,
        BattleNpcSubKind.None => false,
        BattleNpcSubKind.Pet => true,
        _ => false
    };
}