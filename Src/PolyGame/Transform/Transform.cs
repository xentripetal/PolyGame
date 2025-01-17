using System.Collections.Specialized;
using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using PolyECS;

namespace PolyGame.Transform;

[RequiredComponent<Rotation2D>]
[RequiredComponent<Scale2D>]
[RequiredComponent<GlobalTransform2D>]
public record struct Position2D(Vector2 Value)
{
    [ComponentMembers<Position2D>]
    public static void Register(UntypedComponent component)
    {
        component.Member<Vector2>("Value");
    }

    public static implicit operator Vector2(Position2D pos) => pos.Value;
    public static implicit operator Position2D(Vector2 pos) => new(pos);
}

public record struct Rotation2D
{
    public Rotation2D(float degrees) => Degrees = degrees;

    public Rotation2D()
    {
        Degrees = 0;
    }

    public float Radians
    {
        get => MathHelper.ToRadians(Degrees);
        set => Degrees = MathHelper.ToDegrees(value);
    }

    public float Degrees;

    [ComponentMembers]
    public static void Register(UntypedComponent component)
    {
        component.Member<float>("Degrees");
    }
}

public record struct Scale2D
{
    public Vector2 Value;
    public Scale2D(Vector2 scale) => Value = scale;
    public Scale2D() => Value = Vector2.One;

    [ComponentMembers<Scale2D>]
    public static void Register(UntypedComponent component)
    {
        component.Member<Vector2>("Value");
    }

    public static implicit operator Vector2(Scale2D scale) => scale.Value;
    public static implicit operator Scale2D(Vector2 scale) => new(scale);
}

public record struct GlobalTransform2D(Affine2 Value)
{
    [ComponentMembers<GlobalTransform2D>]
    public static void Register(PolyWorld world, UntypedComponent component)
    {
        component.Member<Affine2>("Value");
    }

    public static implicit operator Affine2(GlobalTransform2D transform) => transform.Value;
    public static implicit operator GlobalTransform2D(Affine2 transform) => new(transform);
}