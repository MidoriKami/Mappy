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

    public static Map? GetParentMap()
    {
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var currentMap }) return null;

        if (currentMap.Id.RawString.Split('/') is [_, _] idSplit)
        {
            var index = int.Parse(idSplit[1]);
            
            return LuminaCache<Map>.Instance.FirstOrDefault(map => map.Id.RawString == $"{idSplit[0]}/{index - 1:D2}");
        }

        return null;
    }
}

