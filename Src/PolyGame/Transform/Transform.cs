using Microsoft.Xna.Framework;

namespace PolyGame.Components.Transform;

public record struct Position(Vector3 Value);
public record struct GlobalPosition(Vector3 Value);
public record struct Rotation(Quaternion Value);
public record struct GlobalRotation(Quaternion Value);
public record struct Scale(Vector3 Value);
public record struct GlobalScale(Vector3 Value);

public record struct Position2D(Vector2 Value)
{
    public static implicit operator Vector2(Position2D pos) => pos.Value;
}

public record struct GlobalPosition2D(Vector2 Value)
{
    public static implicit operator Vector2(GlobalPosition2D pos) => pos.Value;
}

public record struct Rotation2D(float Value)
{
    public static implicit operator float(Rotation2D rot) => rot.Value;
}

public record struct GlobalRotation2D(float Value)
{
    public static implicit operator float(GlobalRotation2D rot) => rot.Value;
}

public record struct Scale2D(Vector2 Value)
{
    public static implicit operator Vector2(Scale2D scale) => scale.Value;
}

public record struct GlobalScale2D(Vector2 Value)
{
    public static implicit operator Vector2(GlobalScale2D scale) => scale.Value;
}
