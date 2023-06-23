using System.Drawing;
using System.Numerics;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using Mappy.Views.Attributes;

namespace Mappy.System.Modules;

public class AllianceConfig : IconModuleConfigBase
{
    [ColorConfigOption("TooltipColor", "ModuleConfig", 0, 0.133f, 0.545f, 0.133f, 1.0f)]
    public Vector4 TooltipColor = KnownColor.ForestGreen.AsVector4();
    
    [IconSelection(null, "IconSelection", 0, 60358, 60359, 60360, 60361)]
    public uint SelectedIcon = 60358;
}

public unsafe class AllianceMember : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.AllianceMembers;
    public override ModuleConfigBase Configuration { get; protected set; } = new AllianceConfig();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!DutyLists.Instance.IsType(Service.ClientState.TerritoryType, DutyType.Alliance)) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    public override void LoadForMap(MapData mapData)
    {
        // Do Nothing.
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<AllianceConfig>();
        
        foreach (var member in GroupManager.Instance()->AllianceMembersSpan)
        {
            if (member.ObjectID is 0xE0000000 or 0) continue;
            
            var memberPosition = new Vector2(member.X, member.Z);
            var objectPosition = Position.GetObjectPosition(memberPosition, map);
            
            if(config.ShowIcon) DrawUtilities.DrawIcon(config.SelectedIcon, objectPosition, config.IconScale);
            if(config.ShowTooltip) DrawUtilities.DrawTooltip(MemoryHelper.ReadStringNullTerminated((nint)member.Name), config.TooltipColor);
        }
    }
}