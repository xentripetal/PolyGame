using Microsoft.Xna.Framework;

namespace PolyGame.Physics;

public static class ShapeHelper
{
    public static Vector2[] BuildSymmetricalPolygon(int vertCount, float radius)
    {
        var verts = new Vector2[vertCount];

        for (var i = 0; i < vertCount; i++)
        {
            var a = 2.0f * MathHelper.Pi * (i / (float)vertCount);
            verts[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        }

        return verts;
    }
}
