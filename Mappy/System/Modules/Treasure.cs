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
using ClientStructGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace Mappy.System.Modules;

public class TreasureConfig : IconModuleConfigBase
{
    [ColorConfigOption("TooltipColor", "ModuleColors", 1, 255, 255, 255, 255)]
    public Vector4 TooltipColor = KnownColor.White.AsVector4();

    [IconSelection(null, "IconSelection", 2, 60003, 60354)]
    public uint SelectedIcon = 60003;
}

public class Treasure : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.TreasureMarkers;
    public override ModuleConfigBase Configuration { get; protected set; } = new TreasureConfig();
    
    public override void LoadForMap(MapData mapData)
    {
        // Do Nothing.
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        foreach (var obj in Service.ObjectTable)
        {
            if(obj.ObjectKind != ObjectKind.Treasure) continue;
            var config = GetConfig<TreasureConfig>();

            if(!IsTargetable(obj)) continue;
            
            if(config.ShowIcon) DrawUtilities.DrawIcon(config.SelectedIcon, obj, map, config.IconScale);
            if(config.ShowTooltip) DrawTooltip(obj);
        }
    }
    
    private void DrawTooltip(GameObject gameObject)
    {
        if (!ImGui.IsItemHovered()) return;
        var config = GetConfig<TreasureConfig>();
        
        if (gameObject.Name.TextValue is {Length: > 0} name)
        {
            DrawUtilities.DrawTooltip(name, config.TooltipColor);
        }
    }

    private unsafe bool IsTargetable(GameObject gameObject)
    {
        if (gameObject.Address == nint.Zero) return false;

        if (Service.ClientState.LocalPlayer is not { } player) return false;
        
        if (Vector3.Distance(player.Position, gameObject.Position) < 20.0f)
        {
            return ((ClientStructGameObject*) gameObject.Address)->GetIsTargetable();
        }

        return false;
    }
}