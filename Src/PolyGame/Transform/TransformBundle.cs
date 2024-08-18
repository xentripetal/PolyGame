using Flecs.NET.Core;
using Microsoft.Xna.Framework;

namespace PolyGame.Transform;

public class TransformBundle 
{
    public Vector2 Position = Vector2.Zero;
    public float Rotation = 0;
    public Vector2 Scale = Vector2.One;

    public TransformBundle(Vector2 position, float degrees, Vector2 scale)
    {
        Position = position;
        Rotation = degrees;
        Scale = scale;
    }


    public TransformBundle(Vector2 position = default, float degrees = 0)
    {
        Position = position;
        Rotation = degrees;
        Scale = Vector2.One;
    }

    public TransformBundle()
    {
        Position = Vector2.Zero;
        Rotation = 0;
        Scale = Vector2.One;
    }
    

    public Entity Apply(Entity entity)
    {
        return entity.Set(new Position2D(Position))
            .Set(new Rotation2D(Rotation))
            .Set(new Scale2D(Scale))
            .Add<GlobalTransform2D>();
    }
}
