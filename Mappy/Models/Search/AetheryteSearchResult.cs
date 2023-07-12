using System;
using System.Numerics;
using KamiLib.Caching;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;

namespace Mappy.Models.Search;

public class AetheryteSearchResult : ISearchResult
{
    public string Label { get; set; }
    public string SubLabel { get; set; }
    public uint IconId { get; set; }
    public Vector2 MapPosition { get; set; }
    public float MapZoom { get; set; }
    public uint MapId { get; set; }
    
    public AetheryteSearchResult(uint mapMarkerId, uint subRowId)
    {
        if (LuminaCache<MapMarker>.Instance.GetRow(mapMarkerId, subRowId) is not {
                PlaceNameSubtext.Value.Name.RawString: var aetheryteName, 
                Icon: var iconId,
                X: var xPos,
                Y: var yPos,
                DataKey: var aetheryteId
            } ) throw new Exception("Invalid MapMarker Entry");

        if (LuminaCache<Aetheryte>.Instance.GetRow(aetheryteId) is not
            {
                Map.Row: var mapId, 
                PlaceName.Value.Name.RawString: var mapName
            }) throw new Exception("Invalid Aetheryte Entry");
        
        Label = aetheryteName;
        SubLabel = mapName;
        IconId = iconId;
        MapPosition = new Vector2(xPos, yPos);
        MapZoom = 0.50f;
        MapId = mapId;
    }
}