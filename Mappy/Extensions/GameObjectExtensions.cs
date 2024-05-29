using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Mappy.Classes;

namespace Mappy.Extensions;

public static class GameObjectExtensions {
    public static Vector2 GetMapPosition(this GameObject obj) 
        => new Vector2(obj.Position.X, obj.Position.Z) * DrawHelpers.GetMapScaleFactor();
}