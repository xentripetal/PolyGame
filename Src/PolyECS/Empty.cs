namespace PolyECS;

/// <summary>Empty value for Void/() style generics</summary>
public sealed class Empty
{
    private Empty() { }
    public static Empty Instance { get; } = new();
}
