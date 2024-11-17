using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using KamiLib.Window;
using Lumina.Excel.Sheets;
using Mappy.Classes.SelectionWindowComponents;
using Aetheryte = Lumina.Excel.Sheets.Aetheryte;

namespace Mappy.Windows;

public class MapSelectionWindow : SelectionWindowBase<DrawableOption> {
    
    protected override bool AllowMultiSelect => false;
    
    protected override float SelectionHeight => 75.0f * ImGuiHelpers.GlobalScale;

    public MapSelectionWindow() : base(new Vector2(500.0f, 800.0f)) {
        var maps = Service.DataManager.GetExcelSheet<Map>()
            .Where(map => map is { PlaceName.RowId: not 0, TerritoryType.ValueNullable.LoadingImage.RowId: not 0, TerritoryType.Value.LoadingImage.RowId: not 0 })
            .Where(map => map is not { PriorityUI: 0, PriorityCategoryUI: 0 } )
            .Select(map => new MapDrawableOption {
                Map = map,
            })
            .OfType<DrawableOption>()
            .ToList();

        var poi = Service.DataManager.GetSubrowExcelSheet<MapMarker>()
            .SelectMany(subRowCollection => subRowCollection)
            .Where(marker => marker.PlaceNameSubtext.RowId is not 0)
            .Where(marker => marker.Icon is 60442)
            .Select(marker => new PoiDrawableOption {
                MapMarker = marker,
            })
            .OfType<DrawableOption>()
            .ToList();

        var aetherytes = Service.DataManager.GetExcelSheet<Aetheryte>()
            .Where(aetheryte => aetheryte is not { PlaceName.RowId: 0, AethernetName.RowId: 0 })
            .Where(aetheryte => aetheryte is not { AethernetGroup: 0, Map.RowId: 0 })
            .Select(aetheryte => new AetheryteDrawableOption {
                Aetheryte = aetheryte,
            })
            .OfType<DrawableOption>()
            .ToList();

        SelectionOptions = maps
            .Concat(poi)
            .Concat(aetherytes)
            .ToList();
    }
    
    protected override void DrawSelection(DrawableOption option) {
        option.Draw();
    }

    protected override IEnumerable<string> GetFilterStrings(DrawableOption option)
        => option.GetFilterStrings();
}