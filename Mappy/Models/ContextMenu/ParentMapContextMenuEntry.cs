using System.Linq;
using System.Numerics;
using KamiLib.Caching;
using Mappy.Abstracts;
using Mappy.Models.Enums;
using Mappy.System;
using Map = Lumina.Excel.GeneratedSheets.Map;

namespace Mappy.Models.ContextMenu;

public class ParentMapContextMenuEntry : IContextMenuEntry
{
    public PopupMenuType Type => PopupMenuType.ViewParentMap;
    public string Label => $"View Map: {GetParentMapName()}";
    public bool Enabled => GetParentMap() is not null;
    
    public void ClickAction(Vector2 clickPosition)
    {
        if (MappySystem.MapTextureController is not { Ready: true } textureController) return;

        if (GetParentMap() is { RowId: var mapId })
        {
            textureController.LoadMap(mapId);
            MappySystem.SystemConfig.FollowPlayer = false;
        }
    }

    private static string GetParentMapName() 
        => GetParentMap()?.PlaceName.Value?.Name.RawString ?? "Unable to read map data";

    private static Map? GetParentMap()
    {
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var currentMap }) return null;

        return LuminaCache<Map>.Instance.FirstOrDefault(map => map.TerritoryType.Row == currentMap.TerritoryType.Row && map is { PriorityCategoryUI: not 0, PriorityUI: not 0 });
    }
}

