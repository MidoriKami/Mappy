using System;
using System.Numerics;
using KamiLib.Caching;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;

namespace Mappy.Models.Search;

public class MapSearchResult : ISearchResult
{
    public string Label { get; set; }
    public string SubLabel { get; set; }
    public uint IconId { get; set; }
    public Vector2 MapPosition { get; set; }
    public float MapZoom { get; set; }
    public uint MapId { get; set; }

    public MapSearchResult(uint rowId)
    {
        if (LuminaCache<Map>.Instance.GetRow(rowId) is not
            {
                PlaceName.Value.Name.RawString: var mapName
            }) throw new Exception("Invalid Map Entry");
        
        Label = mapName;
        SubLabel = string.Empty;
        IconId = 60652;
        MapPosition = new Vector2(1024.0f, 1024.0f);
        MapZoom = 0.25f;
        MapId = rowId;
    }
}