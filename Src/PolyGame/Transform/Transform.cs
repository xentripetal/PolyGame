using Microsoft.Xna.Framework;

namespace PolyGame.Transform;

public record struct Position2D(Vector2 Value)
{
    public static implicit operator Vector2(Position2D pos) => pos.Value;
    public static implicit operator Position2D(Vector2 pos) => new (pos);
}

public record struct Rotation2D(float Value)
{
    public static implicit operator float(Rotation2D rot) => rot.Value;
    public static implicit operator Rotation2D(float rot) => new (rot);
}

public record struct Scale2D(Vector2 Value)
{
    public static implicit operator Vector2(Scale2D scale) => scale.Value;
    public static implicit operator Scale2D(Vector2 scale) => new (scale);
}

public record struct GlobalTransform2D(Matrix2D Value)
{
    public static implicit operator Matrix2D(GlobalTransform2D transform) => transform.Value;
    public static implicit operator GlobalTransform2D(Matrix2D transform) => new (transform);
}
