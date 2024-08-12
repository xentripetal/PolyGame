using Flecs.NET.Core;
using Microsoft.Xna.Framework;

namespace PolyGame.Transform;

public struct TransformBundle
{
    public Vector2 Position = Vector2.Zero;
    public float Rotation = 0;
    public Vector2 Scale = Vector2.One;

    public TransformBundle() { }

    public void Apply(Entity entity)
    {
        entity.Set(new Position2D(Position))
            .Set(new Rotation2D(Rotation))
            .Set(new Scale2D(Scale))
            .Add<GlobalTransform2D>();
    }
}
