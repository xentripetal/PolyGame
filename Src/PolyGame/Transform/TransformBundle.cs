using Flecs.NET.Core;
using Microsoft.Xna.Framework;

namespace PolyGame.Transform;

public class TransformBundle
{
    public Vector2 Position;
    public float Rotation;
    public Vector2 Scale;

    public TransformBundle(Vector2 position = default, float degrees = 0, Vector2? scale = null)
    {
        Position = position;
        Rotation = degrees;
        Scale = scale ?? Vector2.One;
    }


    public Entity Apply(Entity entity)
    {
        return entity.Set(new Position2D(Position))
            .Set(new Rotation2D(Rotation))
            .Set(new Scale2D(Scale))
            .Add<GlobalTransform2D>();
    }
}
