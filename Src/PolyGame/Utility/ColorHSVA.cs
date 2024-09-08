using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace PolyGame;

  public struct ColorHSVA : IEquatable<ColorHSVA>
  {
    public float H;
    public float S;
    public float V;
    public float A;

    public ColorHSVA(float h, float s, float v, float a)
    {
      this.H = h;
      this.S = s;
      this.V = v;
      this.A = a;
    }

    public readonly Color ToRGBA()
    {
      float num1 = this.V * this.S;
      float num2 = this.H * 6f;
      float num3 = num1 * (1f - Math.Abs((float) ((double) num2 % 2.0 - 1.0)));
      float num4;
      float num5;
      float num6;
      if ((double) num2 >= 0.0 && (double) num2 < 1.0)
      {
        num4 = num1;
        num5 = num3;
        num6 = 0.0f;
      }
      else if ((double) num2 >= 1.0 && (double) num2 < 2.0)
      {
        num4 = num3;
        num5 = num1;
        num6 = 0.0f;
      }
      else if ((double) num2 >= 2.0 && (double) num2 < 3.0)
      {
        num4 = 0.0f;
        num5 = num1;
        num6 = num3;
      }
      else if ((double) num2 >= 3.0 && (double) num2 < 4.0)
      {
        num4 = 0.0f;
        num5 = num3;
        num6 = num1;
      }
      else if ((double) num2 >= 4.0 && (double) num2 < 5.0)
      {
        num4 = num3;
        num5 = 0.0f;
        num6 = num1;
      }
      else
      {
        num4 = num1;
        num5 = 0.0f;
        num6 = num3;
      }
      float num7 = this.V - num1;
      return new Color(num4 + num7, num5 + num7, num6 + num7, this.A);
    }

    public readonly Vector4 ToVector4() => new Vector4(this.H, this.S, this.V, this.A);

    public override readonly bool Equals(object? obj)
    {
      return obj is ColorHSVA other && this.Equals(other);
    }

    public readonly bool Equals(ColorHSVA other)
    {
      return (double) this.H == (double) other.H && (double) this.S == (double) other.S && (double) this.V == (double) other.V && (double) this.A == (double) other.A;
    }

    public override readonly int GetHashCode()
    {
      return HashCode.Combine<float, float, float, float>(this.H, this.S, this.V, this.A);
    }

    public static bool operator ==(ColorHSVA left, ColorHSVA right) => left.Equals(right);

    public static bool operator !=(ColorHSVA left, ColorHSVA right) => !(left == right);

    public static explicit operator Color(ColorHSVA color) => color.ToRGBA();

    //public static explicit operator ColorHSVA(Color color) => color.ToHSVA();

    public override readonly string ToString()
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 4);
      interpolatedStringHandler.AppendLiteral("<H: ");
      interpolatedStringHandler.AppendFormatted<float>(this.H);
      interpolatedStringHandler.AppendLiteral(", S: ");
      interpolatedStringHandler.AppendFormatted<float>(this.S);
      interpolatedStringHandler.AppendLiteral(", V: ");
      interpolatedStringHandler.AppendFormatted<float>(this.V);
      interpolatedStringHandler.AppendLiteral(", A: ");
      interpolatedStringHandler.AppendFormatted<float>(this.A);
      interpolatedStringHandler.AppendLiteral(">");
      return interpolatedStringHandler.ToStringAndClear();
    }
  }

