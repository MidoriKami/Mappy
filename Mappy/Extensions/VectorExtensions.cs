using System.Numerics;

namespace Mappy.Extensions;

public static class VectorExtensions
{
    public static Vector2 AsMapVector(this Vector3 vector) => new(vector.X, vector.Z);
}