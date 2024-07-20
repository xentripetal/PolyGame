namespace PolyGame.Components;

public struct Managed<T> where T : class
{
    public T Value;

    public Managed(T value)
    {
        Value = value;
    }

    public static implicit operator T(Managed<T> managed)
    {
        return managed.Value;
    }

    public static implicit operator Managed<T>(T value)
    {
        return new Managed<T>(value);
    }
}
