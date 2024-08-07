using Flecs.NET.Core;
using PolyECS;

namespace PolyGame;

public struct Range<T> : IComponent where T : struct, IComparable<T>, IEquatable<T>
{
    public T Start;
    public T End;

    public Range(T start, T end)
    {
        Start = start;
        End = end;
    }

    public override bool Equals(object obj) => obj is Range<T> other && Equals(other);

    public bool Contains(T value) => Start.CompareTo(value) <= 0 && End.CompareTo(value) >= 0;
    public bool Excludes(T value) => Start.CompareTo(value) > 0 || End.CompareTo(value) < 0;
    public bool Equals(Range<T> other) => Start.Equals(other.Start) && End.Equals(other.End);

    public override int GetHashCode() => HashCode.Combine(Start, End);

    public static void Register(UntypedComponent component)
    {
        component.Member<T>("Start").Member<T>("End");
    }
}
