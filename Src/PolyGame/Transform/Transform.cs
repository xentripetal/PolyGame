using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using PolyECS;

namespace PolyGame.Transform;

public record struct Position2D(Vector2 Value) : IComponent
{
    public static void Register(UntypedComponent component)
    {
        component.Member<Vector2>("Value");
    }

    public static implicit operator Vector2(Position2D pos) => pos.Value;
    public static implicit operator Position2D(Vector2 pos) => new (pos);
}

public record struct Rotation2D : IComponent
{
    public Rotation2D(float degrees) => Degrees = degrees;

    public float Radians
    {
        get => MathHelper.ToRadians(Degrees);
        set => Degrees = MathHelper.ToDegrees(value);
    }

    public float Degrees { get; set; }

    public static void Register(UntypedComponent component)
    {
        component.Member<float, Ecs.Units.Angles.Degrees>("Value");
    }
}

public record struct Scale2D(Vector2 Value) : IComponent
{
    public static void Register(UntypedComponent component)
    {
        component.Member<Vector2>("Value");
    }

    public static implicit operator Vector2(Scale2D scale) => scale.Value;
    public static implicit operator Scale2D(Vector2 scale) => new (scale);
}

public record struct GlobalTransform2D(Affine2 Value) : IComponent
{
    public static void Register(UntypedComponent component)
    {
        component.Member<Affine2>("Value");
    }

    public static implicit operator Affine2(GlobalTransform2D transform) => transform.Value;
    public static implicit operator GlobalTransform2D(Affine2 transform) => new (transform);
}
