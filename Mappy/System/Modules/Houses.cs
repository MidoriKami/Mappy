using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Utilities;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.System.Localization;
using Mappy.Utility;

namespace Mappy.System.Modules;

[Category("ModuleConfig")]
public class HousingConfig : IModuleConfig, IIconConfig, ITooltipConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 2;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.65f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.White.AsVector4();

    [BoolConfig("ShowHousingNumber")]
    public bool ShowHousingNumber { get; set; } = true;
}

public unsafe class Houses : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.HousingMarkers;
    public override IModuleConfig Configuration { get; protected set; } = new HousingConfig();
    
    private readonly ConcurrentBag<HousingMapMarkerInfo> housingMarkers = new();

    private bool isHousingDistrict;

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!isHousingDistrict) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    public override void LoadForMap(MapData mapData) => Task.Run(() =>
    {
        housingMarkers.Clear();
        isHousingDistrict = GetHousingDistrictID(mapData.Map) != uint.MaxValue;

        if (!isHousingDistrict) return;

        foreach (var marker in LuminaCache<HousingMapMarkerInfo>.Instance.Where(info => info.Map.Row == mapData.Map.RowId))
        {
            housingMarkers.Add(marker);
        }
    });

    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<HousingConfig>();
        
        foreach (var marker in housingMarkers)
        {
            DrawUtilities.DrawMapIcon(new MappyMapIcon
            {
                IconId = GetIconId(marker, map),
                ObjectPosition = new Vector2(marker.X, marker.Z),
                IconScale = config.IconScale + 0.15f,
                ShowIcon = config.ShowIcon,
            
                Tooltip = GetTooltip(marker),
                ShowTooltip = config.ShowTooltip,
            }, viewport, map);

            if (config.ShowHousingNumber)
            {
                DrawUtilities.DrawMapText(new MappyMapText
                {
                    Text = $"{marker.SubRowId + 1}",
                    ObjectPosition = new Vector2(marker.X, marker.Z) + new Vector2(4.0f, 4.0f),
                
                }, viewport, map);
            }
        }
    }
    
    private uint GetIconId(ExcelRow marker, ExcelRow map)
    {
        if (GetHousingLandSet(map) is not {} housingSizeInfo) return 0;

        return marker.SubRowId switch
        {
            60 when IsHousingManagerValid() => (uint) HousingManager.Instance()->HousingOutdoorTerritory->GetPlotIcon(128),
            61 when IsHousingManagerValid() => (uint) HousingManager.Instance()->HousingOutdoorTerritory->GetPlotIcon(129),
            _  when IsHousingManagerValid() => (uint) HousingManager.Instance()->HousingOutdoorTerritory->GetPlotIcon((byte) marker.SubRowId),
                
            60 when !IsHousingManagerValid() => 60789,
            61 when !IsHousingManagerValid() => 60789,
            _ when !IsHousingManagerValid() => housingSizeInfo.PlotSize[marker.SubRowId] switch {
                0 => 60754, // Small House
                1 => 60755, // Medium House
                2 => 60756, // Large House
                _ => 60750  // Housing Placeholder Marker
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string GetTooltip(ExcelRow marker) => marker.SubRowId switch
    {
        60 or 61 => Strings.Apartment,
        _ => $"{Strings.Plot} {marker.SubRowId + 1}"
    };

    private bool IsHousingManagerValid()
    {
        if (HousingManager.Instance() is null) return false;
        if (HousingManager.Instance()->HousingOutdoorTerritory is null) return false;

        return true;
    }

    private static HousingLandSet? GetHousingLandSet(ExcelRow map) => LuminaCache<HousingLandSet>.Instance.GetRow(GetHousingDistrictID(map));
    
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