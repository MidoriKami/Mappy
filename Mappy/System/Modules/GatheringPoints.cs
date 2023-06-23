using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using ClientStructGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace Mappy.System.Modules;

public class GatheringPointConfig : IconModuleConfigBase
{
    [ColorConfigOption("TooltipColor", "ModuleColors", 1, 1.0f, 1.0f, 1.0f, 1.0f)]
    public Vector4 TooltipColor = KnownColor.White.AsVector4();
}

public class GatheringPoints : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.GatheringPoint;
    public override ModuleConfigBase Configuration { get; protected set; } = new GatheringPointConfig();
    
    public override void LoadForMap(MapData mapData)
    {
        // Do Nothing.
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<GatheringPointConfig>();
        
        foreach (var obj in Service.ObjectTable)
        {
            if(obj.ObjectKind != ObjectKind.GatheringPoint) continue;

            if(!IsTargetable(obj)) continue;
            
            var iconId = GetIconIdForGatheringNode(obj);
            
            if(config.ShowIcon) DrawUtilities.DrawIcon(iconId, obj, map, config.IconScale);
            if(config.ShowTooltip) DrawTooltip(obj);
        }
    }
    
    private void DrawTooltip(GameObject gameObject)
    {
        if (!ImGui.IsItemHovered()) return;
        var config = GetConfig<GatheringPointConfig>();

        var gatheringPoint = LuminaCache<GatheringPoint>.Instance.GetRow(gameObject.DataId)!;
        var gatheringPointBase = LuminaCache<GatheringPointBase>.Instance.GetRow(gatheringPoint.GatheringPointBase.Row)!;
        
        var displayString = $"Level {gatheringPointBase.GatheringLevel} {gameObject.Name.TextValue}";
        if (displayString != string.Empty) DrawUtilities.DrawTooltip(displayString, config.TooltipColor);
    }

    private unsafe bool IsTargetable(GameObject gameObject)
    {
        if (gameObject.Address == IntPtr.Zero) return false;

        var csObject = (ClientStructGameObject*)gameObject.Address;
        return csObject->GetIsTargetable();
    }
    
    private uint GetIconIdForGatheringNode(GameObject gameObject)
    {
        var gatheringPoint = LuminaCache<GatheringPoint>.Instance.GetRow(gameObject.DataId)!;
        var gatheringPointBase = LuminaCache<GatheringPointBase>.Instance.GetRow(gatheringPoint.GatheringPointBase.Row)!;

        return gatheringPointBase.GatheringType.Row switch
        {
            0 => 60438,
            1 => 60437,
            2 => 60433,
            3 => 60432,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}