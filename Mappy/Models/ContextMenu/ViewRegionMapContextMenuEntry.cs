using System.Linq;
using System.Numerics;
using KamiLib.Caching;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models.Enums;
using Mappy.System;

namespace Mappy.Models.ContextMenu;

public class ViewRegionMapContextMenuEntry : IContextMenuEntry
{
    public PopupMenuType Type => PopupMenuType.ViewRegionMap;
    public string Label => "View Region Map";
    public bool Enabled => MappySystem.MapTextureController.CurrentMap?.TerritoryType.Row != 0;
    
    public void ClickAction(Vector2 clickPosition)
    {
        if (MappySystem.MapTextureController.CurrentMap is { PlaceNameRegion.Row: var currentMapRegion })
        {
            if (LuminaCache<Map>.Instance.FirstOrDefault(map => map.PlaceName.Row == currentMapRegion) is { } targetMap)
            {
                MappySystem.MapTextureController.LoadMap(targetMap.RowId);
            }
        }
    }
}