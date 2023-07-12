using System;
using System.Linq;
using System.Numerics;
using KamiLib.Caching;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;

namespace Mappy.Models.Search;

public class PointOfInterestSearchResult : ISearchResult
{
    public string Label { get; set; }
    public string SubLabel { get; set; }
    public uint IconId { get; set; }
    public Vector2 MapPosition { get; set; }
    public float MapZoom { get; set; }
    public uint MapId { get; set; }

    public PointOfInterestSearchResult(uint rowId, uint subRowId)
    {
        if (LuminaCache<MapMarker>.Instance.GetRow(rowId, subRowId) is not {
                PlaceNameSubtext.Value.Name.RawString: var aetheryteName, 
                Icon: var iconId,
                X: var xPos,
                Y: var yPos,
            } ) throw new Exception("Invalid MapMarker Entry");
        
        if(LuminaCache<Map>.Instance.FirstOrDefault(map => map.MapMarkerRange == rowId) is not
           {
               RowId: var mapId,
               PlaceName.Value.Name.RawString: var mapName,
           } ) throw new Exception($"Invalid Map Entry. {rowId}, {subRowId}");
        
        Label = aetheryteName;
        SubLabel = mapName;
        IconId = iconId;
        MapPosition = new Vector2(xPos, yPos);
        MapZoom = 0.50f;
        MapId = mapId;
    }
}