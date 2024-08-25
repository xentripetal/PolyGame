using Microsoft.Xna.Framework;

namespace PolyGame;

public static class VectorExt
{
    public static Vector2 XY(this Vector3 v) => new (v.X, v.Y);
    public static Vector2 XZ(this Vector3 v) => new (v.X, v.Z);
    public static Vector2 YZ(this Vector3 v) => new (v.Y, v.Z);
}
