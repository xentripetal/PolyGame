using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;

namespace PolyGame;

#nullable disable
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct UPoint : IEquatable<UPoint>
{
    [DataMember]
    public uint X;
    [DataMember]
    public uint Y;

    public static UPoint Zero { get; }

    internal string DebugDisplayString => X + "  " + Y;

    public UPoint(uint x, uint y)
    {
        X = x;
        Y = y;
    }

    public UPoint(uint value)
    {
        X = value;
        Y = value;
    }

    public static UPoint operator +(UPoint value1, UPoint value2) => new (value1.X + value2.X, value1.Y + value2.Y);

    public static UPoint operator -(UPoint value1, UPoint value2) => new (value1.X - value2.X, value1.Y - value2.Y);

    public static UPoint operator *(UPoint value1, UPoint value2) => new (value1.X * value2.X, value1.Y * value2.Y);

    public static UPoint operator /(UPoint source, UPoint divisor) => new (source.X / divisor.X, source.Y / divisor.Y);

    public static bool operator ==(UPoint a, UPoint b) => a.Equals(b);

    public static bool operator !=(UPoint a, UPoint b) => !a.Equals(b);

    public override bool Equals(object obj) => obj is UPoint other && Equals(other);

    public bool Equals(UPoint other) => X == other.X && Y == other.Y;

    public override int GetHashCode() => (17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode();

    public override string ToString() => "{X:" + X + " Y:" + Y + "}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2() => new (X, Y);

    public void Deconstruct(out uint x, out uint y)
    {
        x = X;
        y = Y;
    }
}
