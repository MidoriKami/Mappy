using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using ImGuiNET;
using KamiLib.Caching;
using KamiLib.Utilities;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;

namespace Mappy.System.Modules;

public class HousingConfig : IModuleConfig, IIconConfig, ITooltipConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 2;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.65f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.White.AsVector4();
}

public unsafe class Houses : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.HousingMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new HousingConfig();
    
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
        var iconId = GetIconId(marker);

        var position = Position.GetObjectPosition(new Vector2(marker.X, marker.Z), map);

        DrawUtilities.DrawIcon(iconId, position, config.IconScale + 0.15f);
    }
    
    private uint GetIconId(ExcelRow marker)
    {
        if (housingSizeInfo is null) return 0;
        uint iconId;

        if (IsHousingManagerValid())
        {
            iconId = GetIconID(marker.SubRowId switch
            {
                60 => 128,
                61 => 129,
                _ => marker.SubRowId
            });
        }
        else
        {
            iconId = marker.SubRowId is 60 or 61 ? 60789 : GetIconID(housingSizeInfo.PlotSize[marker.SubRowId]);
        }
        return iconId;
    }

    private void DrawTooltip(ExcelRow marker)
    {
        if (!ImGui.IsItemHovered()) return;
        if (marker.SubRowId is 60 or 61) return;
        var config = GetConfig<HousingConfig>();    
        
        DrawUtilities.DrawTooltip($"Plot {marker.SubRowId + 1, 2}", config.TooltipColor, GetIconId(marker));
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