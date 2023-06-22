using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;

namespace Mappy.Utility;

public class Position
{
    public static Vector2 GetObjectPosition(GameObject gameObject, Map map) => 
        GetObjectPosition(gameObject.Position, map);
    public static Vector2 GetObjectPosition(Vector3 position, Map map) => 
        GetObjectPosition(new Vector2(position.X, position.Z), map);
    public static Vector2 GetObjectPosition(Vector2 position, Map map) => 
        position * (map.SizeFactor / 100.0f) + 
        new Vector2(map.OffsetX, map.OffsetY) * (map.SizeFactor / 100.0f) + 
        new Vector2(2048.0f, 2048.0f) / 2.0f;
    
    public static Vector2 GetTextureOffsetPosition(Vector2 coordinates, Map map) => 
        coordinates * (map.SizeFactor / 100.0f) + 
        new Vector2(map.OffsetX, map.OffsetY) * (map.SizeFactor / 100.0f) + 
        new Vector2(2048.0f, 2048.0f) / 2.0f;
    
    public static Vector2 GetTexturePosition(Vector2 coordinates, Map map, Viewport viewport) => 
        (coordinates / viewport.Scale - viewport.Size / 2.0f / viewport.Scale + viewport.Center - new Vector2(2048.0f, 2048.0f) / 2.0f) / 
        (map.SizeFactor / 100.0f) - 
        new Vector2(map.OffsetX, map.OffsetY);
}