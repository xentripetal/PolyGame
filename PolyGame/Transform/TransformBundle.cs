using Flecs.NET.Core;
using Microsoft.Xna.Framework;

namespace PolyGame.Components.Transform;

public struct TransformBundle
{
    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public TransformBundle() { }

    public void Apply(Entity entity)
    {
        entity.Set(new Position(Position))
            .Set(new Rotation(Rotation))
            .Set(new Scale(Scale));
    }
}

public struct TransformBundle2D
{
    public Vector2 Position = Vector2.Zero;
    public float Rotation = 0;
    public Vector2 Scale = Vector2.One;

    public TransformBundle2D() { }

    public void Apply(Entity entity)
    {
        entity.Set(new Position2D(Position))
            .Set(new Rotation2D(Rotation))
            .Set(new Scale2D(Scale));
    }
}
