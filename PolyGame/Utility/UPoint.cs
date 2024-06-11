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
    private static readonly UPoint zeroPoint;
    [DataMember]
    public uint X;
    [DataMember]
    public uint Y;

    public static UPoint Zero => zeroPoint;

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

    public static UPoint operator +(UPoint value1, UPoint value2)
    {
        return new UPoint(value1.X + value2.X, value1.Y + value2.Y);
    }

    public static UPoint operator -(UPoint value1, UPoint value2)
    {
        return new UPoint(value1.X - value2.X, value1.Y - value2.Y);
    }

    public static UPoint operator *(UPoint value1, UPoint value2)
    {
        return new UPoint(value1.X * value2.X, value1.Y * value2.Y);
    }

    public static UPoint operator /(UPoint source, UPoint divisor)
    {
        return new UPoint(source.X / divisor.X, source.Y / divisor.Y);
    }

    public static bool operator ==(UPoint a, UPoint b) => a.Equals(b);

    public static bool operator !=(UPoint a, UPoint b) => !a.Equals(b);

    public override bool Equals(object obj) => obj is UPoint other && Equals(other);

    public bool Equals(UPoint other) => X == other.X && Y == other.Y;

    public override int GetHashCode()
    {
        return (17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode();
    }

    public override string ToString()
    {
        return "{X:" + X + " Y:" + Y + "}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2() => new (X, Y);

    public void Deconstruct(out uint x, out uint y)
    {
        x = X;
        y = Y;
    }
}
