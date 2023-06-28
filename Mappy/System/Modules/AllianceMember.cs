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

[Category("ModuleConfig")]
public class AllianceConfig : IModuleConfig, IIconConfig, ITooltipConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 7;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.LightGreen.AsVector4();
    
    [IconSelection(60358, 60359, 60360, 60361)]
    public uint SelectedIcon { get; set; } = 60358;
}

public unsafe class AllianceMember : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.AllianceMembers;
    public override IModuleConfig Configuration { get; protected set; } = new AllianceConfig();

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
            if(config.ShowTooltip) DrawUtilities.DrawTooltip(MemoryHelper.ReadStringNullTerminated((nint)member.Name), config.TooltipColor, config.SelectedIcon, member.ClassJob + 62000u);
        }
    }
}