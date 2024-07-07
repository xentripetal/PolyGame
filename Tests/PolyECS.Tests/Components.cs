namespace PolyECS.Tests;

public static class Components
{
    public record struct Position
    {
        public float X;
        public float Y;
    }

    public record struct Velocity
    {
        public float X;
        public float Y;
    }

    public record struct Relationship { }

    public record struct CompA
    {
        public int Value;
    }

    public record struct CompB
    {
        public float Value;
    }

    public record struct CompC
    {
        public string Value;
    }
}
