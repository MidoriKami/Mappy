using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;

namespace Mappy.Utility;

public class Position {
    /// <summary>
    /// Gets the draw position for the GameObject for the given map.
    /// </summary>
    /// <param name="gameObject">GameObject</param>
    /// <param name="map">Map</param>
    /// <returns>Texture Coordinates</returns>
    public static Vector2 GetTexturePosition(GameObject gameObject, Map map) 
        => GetTexturePosition(gameObject.Position, map);
    
    /// <summary>
    /// Gets the draw position for the given world coordinates.
    /// </summary>
    /// <param name="position">Position in world coordinates</param>
    /// <param name="map">Map</param>
    /// <returns>Texture Coordinates</returns>
    public static Vector2 GetTexturePosition(Vector3 position, Map map) 
        => GetTexturePosition(new Vector2(position.X, position.Z), map);
    
    /// <summary>
    /// Gets the draw position for the given X, Y, world coordinates.
    /// </summary>
    /// <param name="coordinates">Position in world coordinates</param>
    /// <param name="map">Map</param>
    /// <returns>Texture Coordinates</returns>
    public static Vector2 GetTexturePosition(Vector2 coordinates, Map map) 
        => coordinates * (map.SizeFactor / 100.0f) + 
           new Vector2(map.OffsetX, map.OffsetY) * (map.SizeFactor / 100.0f) + 
           new Vector2(1024.0f, 1024.0f);
    
    /// <summary>
    /// Gets the screen draw position for the texture coordinates relative to the current viewport position.
    /// </summary>
    /// <param name="coordinates">Texture Coordinates</param>
    /// <param name="map">Map</param>
    /// <param name="viewport">Viewport</param>
    /// <returns>Screen Draw Coordinates</returns>
    public static Vector2 GetRawTexturePosition(Vector2 coordinates, Map map, Viewport viewport) 
        => (coordinates / viewport.Scale - viewport.Size / 2.0f / viewport.Scale + viewport.Center - new Vector2(1024.0f, 1024.0f)) / 
           (map.SizeFactor / 100.0f) - 
           new Vector2(map.OffsetX, map.OffsetY);
    
    /// <summary>
    /// Takes the given map coordinate (Ex. 13.5) and converts it to a world coordinate.
    /// </summary>
    /// <param name="value">Map Coordinate</param>
    /// <param name="scale">Map Scale</param>
    /// <param name="offset">Map X or Y offset</param>
    /// <returns></returns>
    public static float MapToWorld(float value, uint scale, int offset) 
        => - offset *  ( scale / 100.0f ) + 50.0f * (value - 1) * ( scale / 100.0f );

    /// <summary>
    /// Convert the given X, Y map coordinates (Ex. 12.4 10.2) and converts it to a world coordinate.
    /// </summary>
    /// <param name="coordinates">Map Coordinates</param>
    /// <param name="map">Map</param>
    /// <returns>World Coordinate</returns>
    public static Vector2 MapToWorld(Vector2 coordinates, Map map) {
        var scalar = map.SizeFactor / 100.0f;
        
        var xWorldCoord = MapToWorld(coordinates.X, map.SizeFactor, map.OffsetX);
        var yWorldCoord = MapToWorld(coordinates.Y, map.SizeFactor, map.OffsetY);

        var objectPosition = new Vector2(xWorldCoord, yWorldCoord);
        var center = new Vector2(1024.0f, 1024.0f);
        
        return objectPosition / scalar - center / scalar;
    }
}