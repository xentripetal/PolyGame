using Microsoft.Xna.Framework;

namespace PolyGame.Components.Transform;

public struct Transform
{
    /// <summary>
    /// Position of the entity. In 2d, the last value of the Vec3 is used for z-ordering
    /// </summary>
    public Vector3 Translation;
    public Quaternion Quat;
    public Vector3 Scale;

    public Transform(Vector3 translation, Quaternion quat, Vector3 scale)
    {
        Translation = translation;
        Quat = quat;
        Scale = scale;
    }

    public Transform(Matrix matrix)
    {
        matrix.Decompose(out var scale, out var rot, out var translation);
        Translation = translation;
        Quat = rot;
        Scale = scale;
    }
    
    public static Transform Identity => new Transform(Vector3.Zero, Quaternion.Identity, Vector3.One);
}
