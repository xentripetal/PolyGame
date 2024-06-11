using Microsoft.Xna.Framework;

namespace PolyGame.Components.Transform;

/// <summary>
/// Describes the position of an entity relative to the reference frame.
///
/// To place or move an entity, you should modify its <see cref="Transform"/>
/// Global transform is fully managed by bevy, you cannot mutate it.
/// </summary>
public struct GlobalTransform
{
    public Matrix Matrix;

    public Transform ComputeTransform()
    {
        return new Transform(Matrix);
    }
    
    public Vector3 Position => Matrix.Translation;
    public Vector3 Scale => new Vector3(Matrix.M11, Matrix.M22, Matrix.M33);
}
