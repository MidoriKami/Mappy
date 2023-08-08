using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;

namespace Mappy.Utility;

public class Position
{
    public static Vector2 GetTexturePosition(GameObject gameObject, Map map) 
        => GetTexturePosition(gameObject.Position, map);
    
    public static Vector2 GetTexturePosition(Vector3 position, Map map) 
        => GetTexturePosition(new Vector2(position.X, position.Z), map);
    
    public static Vector2 GetTexturePosition(Vector2 coordinates, Map map) 
        => coordinates * (map.SizeFactor / 100.0f) + 
           new Vector2(map.OffsetX, map.OffsetY) * (map.SizeFactor / 100.0f) + 
           new Vector2(1024.0f, 1024.0f);
    
    public static Vector2 GetRawTexturePosition(Vector2 coordinates, Map map, Viewport viewport) 
        => (coordinates / viewport.Scale - viewport.Size / 2.0f / viewport.Scale + viewport.Center - new Vector2(1024.0f, 1024.0f)) / 
           (map.SizeFactor / 100.0f) - 
           new Vector2(map.OffsetX, map.OffsetY);
    
    public static float MapToWorld(float value, uint scale, int offset) 
        => - offset *  ( scale / 100.0f ) + 50.0f * (value - 1) * ( scale / 100.0f );

    public static Vector2 MapToWorld(Vector2 coordinates, Map map)
    {
        var scalar = map.SizeFactor / 100.0f;
        
        var xWorldCoord = MapToWorld(coordinates.X, map.SizeFactor, map.OffsetX);
        var yWorldCoord = MapToWorld(coordinates.Y, map.SizeFactor, map.OffsetY);

        var objectPosition = new Vector2(xWorldCoord, yWorldCoord);
        var center = new Vector2(1024.0f, 1024.0f);
        
        return objectPosition / scalar - center / scalar;
    }
}