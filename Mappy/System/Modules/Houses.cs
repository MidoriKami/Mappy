using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using ImGuiNET;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Utilities;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;

namespace Mappy.System.Modules;

public class HousingConfig : IconModuleConfigBase
{
    [ColorConfigOption("TooltipColor", "ModuleColors", 1, 1.0f, 1.0f, 1.0f, 1.0f)]
    public Vector4 TooltipColor = KnownColor.White.AsVector4();
}

public unsafe class Houses : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.HousingMarkers;
    public override ModuleConfigBase Configuration { get; protected set; } = new HousingConfig();
    
    private delegate uint GetPlotIconIdDelegate(HousingTerritory* outdoorTerritory, byte plotIndex);

    [Signature("40 56 57 48 83 EC 38 0F B6 FA")]
    private readonly GetPlotIconIdDelegate? getPlotIconId = null;

    private readonly ConcurrentBag<HousingMapMarkerInfo> housingMarkers = new();
    private HousingLandSet? housingSizeInfo;

    private bool isHousingDistrict;

    public Houses() => SignatureHelper.Initialise(this);
    
    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!isHousingDistrict) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    public override void LoadForMap(MapData mapData) => Task.Run(() =>
    {
        housingMarkers.Clear();
        isHousingDistrict = GetHousingDistrictID(mapData.Map) != uint.MaxValue;

        if (isHousingDistrict)
        {
            foreach (var marker in LuminaCache<HousingMapMarkerInfo>.Instance.Where(info => info.Map.Row == mapData.Map.RowId))
            {
                housingMarkers.Add(marker);
            }
            
            housingSizeInfo = LuminaCache<HousingLandSet>.Instance.GetRow(GetHousingDistrictID(mapData.Map));
        }
    });

    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<HousingConfig>();
        
        foreach (var marker in housingMarkers)
        {
            if(config.ShowIcon) DrawHousingMapMarker(marker, map);
            if(config.ShowTooltip) DrawTooltip(marker);
        }
    }
    
    private void DrawHousingMapMarker(HousingMapMarkerInfo marker, Map map)
    {
        if (housingSizeInfo is null) return;
        var config = GetConfig<HousingConfig>();
        uint iconId;
        
        if (IsHousingManagerValid())
        {
            iconId = GetIconID( marker.SubRowId switch
            {
                60 => 128,
                61 => 129,
                _ => marker.SubRowId
            } );
        }
        else
        {
            iconId = marker.SubRowId is 60 or 61 ? 60789 : GetIconID(housingSizeInfo.PlotSize[marker.SubRowId]);
        }
        
        var position = Position.GetObjectPosition(new Vector2(marker.X, marker.Z), map);

        DrawUtilities.DrawIcon(iconId, position, config.IconScale + 0.15f);
    }

    private void DrawTooltip(ExcelRow marker)
    {
        if (!ImGui.IsItemHovered()) return;
        if (marker.SubRowId is 60 or 61) return;
        var config = GetConfig<HousingConfig>();    
        
        DrawUtilities.DrawTooltip($"{marker.SubRowId + 1}", config.TooltipColor);
    }

    private uint GetIconID(uint housingIndex)
    {
        if (!IsHousingManagerValid())
        {
            return housingSizeInfo?.PlotSize[housingIndex] switch
            {
                0 => 60754, // Small House
                1 => 60755, // Medium House
                2 => 60756, // Large House
                _ => 60750  // Housing Placeholder Marker
            };
        }
        
        return getPlotIconId?.Invoke(HousingManager.Instance()->OutdoorTerritory, (byte) housingIndex) ?? 0;
    }

    private bool IsHousingManagerValid()
    {
        if (HousingManager.Instance() is null) return false;
        if (HousingManager.Instance()->OutdoorTerritory is null) return false;

        return true;
    }

    private static uint GetHousingDistrictID(ExcelRow map) => map.RowId switch
    {
        72 or 192 => 0,
        82 or 193 => 1,
        83 or 194 => 2,
        364 or 365 => 3,
        679 or 680 => 4,
        _ => uint.MaxValue
    };
}