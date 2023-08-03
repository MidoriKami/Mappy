using System.Linq;
using System.Numerics;
using KamiLib.Caching;
using Mappy.Abstracts;
using Mappy.Models.Enums;
using Mappy.System;
using Map = Lumina.Excel.GeneratedSheets.Map;

namespace Mappy.Models.ContextMenu;

public class MapLayerContextMenuEntry : IContextMenuEntry
{
    public PopupMenuType Type => PopupMenuType.Layers;
    public string Label => $"View Map: {GetParentArea()}";
    public bool Enabled => IsInSubArea();
    
    public void ClickAction(Vector2 clickPosition)
    {
        if (MappySystem.MapTextureController is not { Ready: true } textureController) return;

        if (GetParentMap() is { RowId: var mapId })
        {
            textureController.LoadMap(mapId);
            MappySystem.SystemConfig.FollowPlayer = false;
        }
    }

    private bool IsInSubArea()
    {
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return false;
        if (map is { PriorityCategoryUI: 0, PriorityUI: 0 }) return true;
        
        return false;
    }

    private string GetParentArea() 
        => GetParentMap()?.PlaceName.Value?.Name.RawString ?? "Unable to read map data";

    private Map? GetParentMap()
    {
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var currentMap }) return null;

        return LuminaCache<Map>.Instance.FirstOrDefault(map => map.TerritoryType.Row == currentMap.TerritoryType.Row && map is { PriorityCategoryUI: not 0, PriorityUI: not 0 });
    }
}

