using System.Diagnostics;
using System.Runtime.CompilerServices;
using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using PolyECS;

namespace PolyGame;

/// <summary>
///     Represents the right-handed 2x2 floating point matrix, which can store scale and rotation information.
/// </summary>
/// <remarks>Copy of Nez.Matrix2D</remarks>
[DebuggerDisplay("{debugDisplayString,nq}")]
public struct Mat2 : IEquatable<Mat2>
{
    #region Public Fields

    public Vector2 XAxis;
    public Vector2 YAxis;
    
    [ComponentMembers<Mat2>]
    public static void RegisterMembers(UntypedComponent component)
    {
        component.Member<Vector2>("XAxis");
        component.Member<Vector2>("YAxis");
    }

    #endregion


    #region Public Properties

    /// <summary>
    ///     Returns the identity matrix.
    /// </summary>
    public static Mat2 Identity { get; } = new (1f, 0f, 0f, 1f);

    /// <summary>
    ///     Returns the zero matrix.
    /// </summary>
    public static Mat2 Zero { get; } = new (0f, 0f, 0f, 0f);

    /// <summary>
    ///     rotation in radians stored in this matrix
    /// </summary>
    /// <value>The rotation.</value>
    public float Rotation
    {
        get => MathF.Atan2(-YAxis.X, YAxis.Y);
        set
        {
            var offset = 2 * MathF.PI - Rotation;
            (var sin, var cos) = MathF.SinCos(value + offset);

            var scale = Scale;
            XAxis.X = cos * scale.X;
            XAxis.Y = sin * scale.X;
            YAxis.X = -sin * scale.Y;
            YAxis.Y = cos * scale.Y;
        }
    }

    /// <summary>
    ///     rotation in degrees stored in this matrix
    /// </summary>
    /// <value>The rotation degrees.</value>
    public float RotationDegrees
    {
        get => MathHelper.ToDegrees(Rotation);
        set => Rotation = MathHelper.ToRadians(value);
    }

    /// <summary>
    ///     Scale stored in this matrix.
    /// </summary>
    public Vector2 Scale
    {
        get => new (XAxis.Length() * Math.Sign(Determinant()), YAxis.Length());
        set
        {
            var oldScale = Scale;
            // hacky way to undo the current scale
            this = CreateScale(-oldScale) * this * CreateScale(value);
        }
    }

    #endregion

    /// <summary>
    ///     Constructs a matrix.
    /// </summary>
    public Mat2(float m11, float m12, float m21, float m22)
    {
        XAxis = new Vector2(m11, m12);
        YAxis = new Vector2(m21, m22);
    }

    public Mat2(Vector2 xAxis, Vector2 yAxis)
    {
        XAxis = xAxis;
        YAxis = yAxis;
    }

    public Mat2(Vector2 scale, float radians)
    {
        (var sin, var cos) = MathF.SinCos(radians);

        XAxis = new Vector2(cos * scale.X, sin * scale.X);
        YAxis = new Vector2(-sin * scale.Y, cos * scale.Y);
    }


    #region Public static methods

    /// <summary>
    ///     Creates a new <see cref="Mat2" /> which contains sum of two matrixes.
    /// </summary>
    /// <param name="matrix1">The first matrix to add.</param>
    /// <param name="matrix2">The second matrix to add.</param>
    /// <returns>The result of the matrix addition.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 Add(Mat2 matrix1, Mat2 matrix2)
    {
        matrix1.XAxis.X += matrix2.XAxis.X;
        matrix1.XAxis.Y += matrix2.XAxis.Y;

        matrix1.YAxis.X += matrix2.YAxis.X;
        matrix1.YAxis.Y += matrix2.YAxis.Y;
        return matrix1;
    }


    /// <summary>
    ///     Creates a new <see cref="Mat2" /> which contains sum of two matrixes.
    /// </summary>
    /// <param name="matrix1">The first matrix to add.</param>
    /// <param name="matrix2">The second matrix to add.</param>
    /// <param name="result">The result of the matrix addition as an output parameter.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(ref Mat2 matrix1, ref Mat2 matrix2, out Mat2 result)
    {
        result.XAxis.X = matrix1.XAxis.X + matrix2.XAxis.X;
        result.XAxis.Y = matrix1.XAxis.Y + matrix2.XAxis.Y;

        result.YAxis.X = matrix1.YAxis.X + matrix2.YAxis.X;
        result.YAxis.Y = matrix1.YAxis.Y + matrix2.YAxis.Y;
    }


    /// <summary>
    ///     Creates a new rotation <see cref="Mat2" /> around Z axis.
    /// </summary>
    /// <param name="radians">Angle in radians.</param>
    /// <returns>The rotation <see cref="Mat2" /> around Z axis.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 CreateRotation(float radians)
    {
        Mat2 result;
        CreateRotation(radians, out result);
        return result;
    }


    /// <summary>
    ///     Creates a new rotation <see cref="Mat2" /> around Z axis.
    /// </summary>
    /// <param name="radians">Angle in radians.</param>
    /// <param name="result">The rotation <see cref="Mat2" /> around Z axis as an output parameter.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreateRotation(float radians, out Mat2 result)
    {
        result = Identity;

        var val1 = MathF.Cos(radians);
        var val2 = MathF.Sin(radians);

        result.XAxis.X = val1;
        result.XAxis.Y = val2;
        result.YAxis.X = -val2;
        result.YAxis.Y = val1;
    }


    /// <summary>
    ///     Creates a new scaling <see cref="Mat2" />.
    /// </summary>
    /// <param name="scale">Scale value for all three axises.</param>
    /// <returns>The scaling <see cref="Mat2" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 CreateScale(float scale)
    {
        Mat2 result;
        CreateScale(scale, scale, out result);
        return result;
    }


    /// <summary>
    ///     Creates a new scaling <see cref="Mat2" />.
    /// </summary>
    /// <param name="scale">Scale value for all three axises.</param>
    /// <param name="result">The scaling <see cref="Mat2" /> as an output parameter.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreateScale(float scale, out Mat2 result)
    {
        CreateScale(scale, scale, out result);
    }


    /// <summary>
    ///     Creates a new scaling <see cref="Mat2" />.
    /// </summary>
    /// <param name="xScale">Scale value for X axis.</param>
    /// <param name="yScale">Scale value for Y axis.</param>
    /// <returns>The scaling <see cref="Mat2" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 CreateScale(float xScale, float yScale)
    {
        Mat2 result;
        CreateScale(xScale, yScale, out result);
        return result;
    }


    /// <summary>
    ///     Creates a new scaling <see cref="Mat2" />.
    /// </summary>
    /// <param name="xScale">Scale value for X axis.</param>
    /// <param name="yScale">Scale value for Y axis.</param>
    /// <param name="result">The scaling <see cref="Mat2" /> as an output parameter.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreateScale(float xScale, float yScale, out Mat2 result)
    {
        result.XAxis.X = xScale;
        result.XAxis.Y = 0;

        result.YAxis.X = 0;
        result.YAxis.Y = yScale;
    }


    /// <summary>
    ///     Creates a new scaling <see cref="Mat2" />.
    /// </summary>
    /// <param name="scale"><see cref="Vector2" /> representing x and y scale values.</param>
    /// <returns>The scaling <see cref="Mat2" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 CreateScale(Vector2 scale)
    {
        Mat2 result;
        CreateScale(ref scale, out result);
        return result;
    }


    /// <summary>
    ///     Creates a new scaling <see cref="Mat2" />.
    /// </summary>
    /// <param name="scale"><see cref="Vector3" /> representing x,y and z scale values.</param>
    /// <param name="result">The scaling <see cref="Mat2" /> as an output parameter.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreateScale(ref Vector2 scale, out Mat2 result)
    {
        result.XAxis.X = scale.X;
        result.XAxis.Y = 0;

        result.YAxis.X = 0;
        result.YAxis.Y = scale.Y;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Determinant() => XAxis.X * YAxis.Y - XAxis.Y * YAxis.X;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Invert(ref Mat2 matrix, out Mat2 result)
    {
        var det = 1 / matrix.Determinant();

        result.XAxis.X = matrix.YAxis.Y * det;
        result.XAxis.Y = -matrix.XAxis.Y * det;

        result.YAxis.X = -matrix.YAxis.X * det;
        result.YAxis.Y = matrix.XAxis.X * det;
    }


    /// <summary>
    ///     Divides the elements of a <see cref="Mat2" /> by the elements of another matrix.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" />.</param>
    /// <param name="matrix2">Divisor <see cref="Mat2" />.</param>
    /// <returns>The result of dividing the matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 Divide(Mat2 matrix1, Mat2 matrix2)
    {
        matrix1.XAxis.X = matrix1.XAxis.X / matrix2.XAxis.X;
        matrix1.XAxis.Y = matrix1.XAxis.Y / matrix2.XAxis.Y;

        matrix1.YAxis.X = matrix1.YAxis.X / matrix2.YAxis.X;
        matrix1.YAxis.Y = matrix1.YAxis.Y / matrix2.YAxis.Y;
        return matrix1;
    }


    /// <summary>
    ///     Divides the elements of a <see cref="Mat2" /> by the elements of another matrix.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" />.</param>
    /// <param name="matrix2">Divisor <see cref="Mat2" />.</param>
    /// <param name="result">The result of dividing the matrix as an output parameter.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Divide(ref Mat2 matrix1, ref Mat2 matrix2, out Mat2 result)
    {
        result.XAxis.X = matrix1.XAxis.X / matrix2.XAxis.X;
        result.XAxis.Y = matrix1.XAxis.Y / matrix2.XAxis.Y;

        result.YAxis.X = matrix1.YAxis.X / matrix2.YAxis.X;
        result.YAxis.Y = matrix1.YAxis.Y / matrix2.YAxis.Y;
    }


    /// <summary>
    ///     Divides the elements of a <see cref="Mat2" /> by a scalar.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" />.</param>
    /// <param name="divider">Divisor scalar.</param>
    /// <returns>The result of dividing a matrix by a scalar.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 Divide(Mat2 matrix1, float divider)
    {
        var num = 1f / divider;
        matrix1.XAxis.X = matrix1.XAxis.X * num;
        matrix1.XAxis.Y = matrix1.XAxis.Y * num;

        matrix1.YAxis.X = matrix1.YAxis.X * num;
        matrix1.YAxis.Y = matrix1.YAxis.Y * num;

        return matrix1;
    }


    /// <summary>
    ///     Divides the elements of a <see cref="Mat2" /> by a scalar.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" />.</param>
    /// <param name="divider">Divisor scalar.</param>
    /// <param name="result">The result of dividing a matrix by a scalar as an output parameter.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Divide(ref Mat2 matrix1, float divider, out Mat2 result)
    {
        var num = 1f / divider;
        result.XAxis.X = matrix1.XAxis.X * num;
        result.XAxis.Y = matrix1.XAxis.Y * num;

        result.YAxis.X = matrix1.YAxis.X * num;
        result.YAxis.Y = matrix1.YAxis.Y * num;
    }


    /// <summary>
    ///     Creates a new <see cref="Mat2" /> that contains linear interpolation of the values in specified matrixes.
    /// </summary>
    /// <param name="matrix1">The first <see cref="Mat2" />.</param>
    /// <param name="matrix2">The second <see cref="Vector2" />.</param>
    /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
    /// <returns>>The result of linear interpolation of the specified matrixes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 Lerp(Mat2 matrix1, Mat2 matrix2, float amount)
    {
        matrix1.XAxis.X = matrix1.XAxis.X + (matrix2.XAxis.X - matrix1.XAxis.X) * amount;
        matrix1.XAxis.Y = matrix1.XAxis.Y + (matrix2.XAxis.Y - matrix1.XAxis.Y) * amount;

        matrix1.YAxis.X = matrix1.YAxis.X + (matrix2.YAxis.X - matrix1.YAxis.X) * amount;
        matrix1.YAxis.Y = matrix1.YAxis.Y + (matrix2.YAxis.Y - matrix1.YAxis.Y) * amount;
        return matrix1;
    }


    /// <summary>
    ///     Creates a new <see cref="Mat2" /> that contains linear interpolation of the values in specified matrixes.
    /// </summary>
    /// <param name="matrix1">The first <see cref="Mat2" />.</param>
    /// <param name="matrix2">The second <see cref="Vector2" />.</param>
    /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
    /// <param name="result">The result of linear interpolation of the specified matrixes as an output parameter.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Lerp(ref Mat2 matrix1, ref Mat2 matrix2, float amount, out Mat2 result)
    {
        result.XAxis.X = matrix1.XAxis.X + (matrix2.XAxis.X - matrix1.XAxis.X) * amount;
        result.XAxis.Y = matrix1.XAxis.Y + (matrix2.XAxis.Y - matrix1.XAxis.Y) * amount;

        result.YAxis.X = matrix1.YAxis.X + (matrix2.YAxis.X - matrix1.YAxis.X) * amount;
        result.YAxis.Y = matrix1.YAxis.Y + (matrix2.YAxis.Y - matrix1.YAxis.Y) * amount;

    }


    /// <summary>
    ///     Creates a new <see cref="Mat2" /> that contains a multiplication of two matrix.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" />.</param>
    /// <param name="matrix2">Source <see cref="Mat2" />.</param>
    /// <returns>Result of the matrix multiplication.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 Multiply(Mat2 matrix1, Mat2 matrix2)
    {
        var m11 = matrix1.XAxis.X * matrix2.XAxis.X + matrix1.XAxis.Y * matrix2.YAxis.X;
        var m12 = matrix1.XAxis.X * matrix2.XAxis.Y + matrix1.XAxis.Y * matrix2.YAxis.Y;

        var m21 = matrix1.YAxis.X * matrix2.XAxis.X + matrix1.YAxis.Y * matrix2.YAxis.X;
        var m22 = matrix1.YAxis.X * matrix2.XAxis.Y + matrix1.YAxis.Y * matrix2.YAxis.Y;

        matrix1.XAxis.X = m11;
        matrix1.XAxis.Y = m12;

        matrix1.YAxis.X = m21;
        matrix1.YAxis.Y = m22;
        return matrix1;
    }

    public static Vector2 operator *(Mat2 mat, Vector2 vec)
    {
        var res = new Vector2();
        Multiply(ref mat, ref vec, out res);
        return res;
    }

    public static void Multiply(in Mat2 matrix, in Vector2 vec, out Vector2 result)
    {
        result.X = matrix.XAxis.X * vec.X + matrix.YAxis.X * vec.Y;
        result.Y = matrix.XAxis.Y * vec.X + matrix.YAxis.Y * vec.Y;
    }


    /// <summary>
    ///     Creates a new <see cref="Mat2" /> that contains a multiplication of two matrix.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" />.</param>
    /// <param name="matrix2">Source <see cref="Mat2" />.</param>
    /// <param name="result">Result of the matrix multiplication as an output parameter.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Multiply(in Mat2 matrix1, in Mat2 matrix2, out Mat2 result)
    {
        var m11 = matrix1.XAxis.X * matrix2.XAxis.X + matrix1.XAxis.Y * matrix2.YAxis.X;
        var m12 = matrix1.XAxis.X * matrix2.XAxis.Y + matrix1.XAxis.Y * matrix2.YAxis.Y;

        var m21 = matrix1.YAxis.X * matrix2.XAxis.X + matrix1.YAxis.Y * matrix2.YAxis.X;
        var m22 = matrix1.YAxis.X * matrix2.XAxis.Y + matrix1.YAxis.Y * matrix2.YAxis.Y;

        result.XAxis.X = m11;
        result.XAxis.Y = m12;

        result.YAxis.X = m21;
        result.YAxis.Y = m22;
    }


    /// <summary>
    ///     Creates a new <see cref="Mat2" /> that contains a multiplication of <see cref="Mat2" /> and a scalar.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" />.</param>
    /// <param name="scaleFactor">Scalar value.</param>
    /// <returns>Result of the matrix multiplication with a scalar.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 Multiply(Mat2 matrix1, float scaleFactor)
    {
        matrix1.XAxis.X *= scaleFactor;
        matrix1.XAxis.Y *= scaleFactor;

        matrix1.YAxis.X *= scaleFactor;
        matrix1.YAxis.Y *= scaleFactor;
        return matrix1;
    }


    /// <summary>
    ///     Creates a new <see cref="Mat2" /> that contains a multiplication of <see cref="Mat2" /> and a scalar.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" />.</param>
    /// <param name="scaleFactor">Scalar value.</param>
    /// <param name="result">Result of the matrix multiplication with a scalar as an output parameter.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Multiply(ref Mat2 matrix1, float scaleFactor, out Mat2 result)
    {
        result.XAxis.X = matrix1.XAxis.X * scaleFactor;
        result.XAxis.Y = matrix1.XAxis.Y * scaleFactor;

        result.YAxis.X = matrix1.YAxis.X * scaleFactor;
        result.YAxis.Y = matrix1.YAxis.Y * scaleFactor;
    }


    /// <summary>
    ///     Adds two matrixes.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" /> on the left of the add sign.</param>
    /// <param name="matrix2">Source <see cref="Mat2" /> on the right of the add sign.</param>
    /// <returns>Sum of the matrixes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 operator +(Mat2 matrix1, Mat2 matrix2)
    {
        matrix1.XAxis.X = matrix1.XAxis.X + matrix2.XAxis.X;
        matrix1.XAxis.Y = matrix1.XAxis.Y + matrix2.XAxis.Y;

        matrix1.YAxis.X = matrix1.YAxis.X + matrix2.YAxis.X;
        matrix1.YAxis.Y = matrix1.YAxis.Y + matrix2.YAxis.Y;
        return matrix1;
    }


    /// <summary>
    ///     Divides the elements of a <see cref="Mat2" /> by the elements of another <see cref="Mat2" />.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" /> on the left of the div sign.</param>
    /// <param name="matrix2">Divisor <see cref="Mat2" /> on the right of the div sign.</param>
    /// <returns>The result of dividing the matrixes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 operator /(Mat2 matrix1, Mat2 matrix2)
    {
        matrix1.XAxis.X = matrix1.XAxis.X / matrix2.XAxis.X;
        matrix1.XAxis.Y = matrix1.XAxis.Y / matrix2.XAxis.Y;

        matrix1.YAxis.X = matrix1.YAxis.X / matrix2.YAxis.X;
        matrix1.YAxis.Y = matrix1.YAxis.Y / matrix2.YAxis.Y;
        return matrix1;
    }


    /// <summary>
    ///     Divides the elements of a <see cref="Mat2" /> by a scalar.
    /// </summary>
    /// <param name="matrix">Source <see cref="Mat2" /> on the left of the div sign.</param>
    /// <param name="divider">Divisor scalar on the right of the div sign.</param>
    /// <returns>The result of dividing a matrix by a scalar.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 operator /(Mat2 matrix, float divider)
    {
        var num = 1f / divider;
        matrix.XAxis.X = matrix.XAxis.X * num;
        matrix.XAxis.Y = matrix.XAxis.Y * num;

        matrix.YAxis.X = matrix.YAxis.X * num;
        matrix.YAxis.Y = matrix.YAxis.Y * num;
        return matrix;
    }


    /// <summary>
    ///     Compares whether two <see cref="Mat2" /> instances are equal without any tolerance.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" /> on the left of the equal sign.</param>
    /// <param name="matrix2">Source <see cref="Mat2" /> on the right of the equal sign.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Mat2 matrix1, Mat2 matrix2) => matrix1.XAxis == matrix2.XAxis &&
                                                                  matrix1.YAxis == matrix2.YAxis;


    /// <summary>
    ///     Compares whether two <see cref="Mat2" /> instances are not equal without any tolerance.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" /> on the left of the not equal sign.</param>
    /// <param name="matrix2">Source <see cref="Mat2" /> on the right of the not equal sign.</param>
    /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Mat2 matrix1, Mat2 matrix2) => matrix1.XAxis != matrix2.XAxis ||
                                                                  matrix1.YAxis != matrix2.YAxis;


    /// <summary>
    ///     Multiplies two matrixes.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" /> on the left of the mul sign.</param>
    /// <param name="matrix2">Source <see cref="Mat2" /> on the right of the mul sign.</param>
    /// <returns>Result of the matrix multiplication.</returns>
    /// <remarks>
    ///     Using matrix multiplication algorithm - see http://en.wikipedia.org/wiki/Matrix_multiplication.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 operator *(Mat2 matrix1, Mat2 matrix2)
    {
        var m11 = matrix1.XAxis.X * matrix2.XAxis.X + matrix1.XAxis.Y * matrix2.YAxis.X;
        var m12 = matrix1.XAxis.X * matrix2.XAxis.Y + matrix1.XAxis.Y * matrix2.YAxis.Y;

        var m21 = matrix1.YAxis.X * matrix2.XAxis.X + matrix1.YAxis.Y * matrix2.YAxis.X;
        var m22 = matrix1.YAxis.X * matrix2.XAxis.Y + matrix1.YAxis.Y * matrix2.YAxis.Y;

        matrix1.XAxis.X = m11;
        matrix1.XAxis.Y = m12;

        matrix1.YAxis.X = m21;
        matrix1.YAxis.Y = m22;

        return matrix1;
    }


    /// <summary>
    ///     Multiplies the elements of matrix by a scalar.
    /// </summary>
    /// <param name="matrix">Source <see cref="Mat2" /> on the left of the mul sign.</param>
    /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
    /// <returns>Result of the matrix multiplication with a scalar.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 operator *(Mat2 matrix, float scaleFactor)
    {
        matrix.XAxis.X = matrix.XAxis.X * scaleFactor;
        matrix.XAxis.Y = matrix.XAxis.Y * scaleFactor;

        matrix.YAxis.X = matrix.YAxis.X * scaleFactor;
        matrix.YAxis.Y = matrix.YAxis.Y * scaleFactor;

        return matrix;
    }


    /// <summary>
    ///     Subtracts the values of one <see cref="Mat2" /> from another <see cref="Mat2" />.
    /// </summary>
    /// <param name="matrix1">Source <see cref="Mat2" /> on the left of the sub sign.</param>
    /// <param name="matrix2">Source <see cref="Mat2" /> on the right of the sub sign.</param>
    /// <returns>Result of the matrix subtraction.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 operator -(Mat2 matrix1, Mat2 matrix2)
    {
        matrix1.XAxis.X = matrix1.XAxis.X - matrix2.XAxis.X;
        matrix1.XAxis.Y = matrix1.XAxis.Y - matrix2.XAxis.Y;

        matrix1.YAxis.X = matrix1.YAxis.X - matrix2.YAxis.X;
        matrix1.YAxis.Y = matrix1.YAxis.Y - matrix2.YAxis.Y;

        return matrix1;
    }


    /// <summary>
    ///     Inverts values in the specified <see cref="Mat2" />.
    /// </summary>
    /// <param name="matrix">Source <see cref="Mat2" /> on the right of the sub sign.</param>
    /// <returns>Result of the inversion.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 operator -(Mat2 matrix)
    {
        matrix.XAxis.X = -matrix.XAxis.X;
        matrix.XAxis.Y = -matrix.XAxis.Y;

        matrix.YAxis.X = -matrix.YAxis.X;
        matrix.YAxis.Y = -matrix.YAxis.Y;

        return matrix;
    }


    /// <summary>
    ///     Creates a new <see cref="Mat2" /> that contains subtraction of one matrix from another.
    /// </summary>
    /// <param name="matrix1">The first <see cref="Mat2" />.</param>
    /// <param name="matrix2">The second <see cref="Mat2" />.</param>
    /// <returns>The result of the matrix subtraction.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 Subtract(Mat2 matrix1, Mat2 matrix2)
    {
        matrix1.XAxis.X = matrix1.XAxis.X - matrix2.XAxis.X;
        matrix1.XAxis.Y = matrix1.XAxis.Y - matrix2.XAxis.Y;

        matrix1.YAxis.X = matrix1.YAxis.X - matrix2.YAxis.X;
        matrix1.YAxis.Y = matrix1.YAxis.Y - matrix2.YAxis.Y;

        return matrix1;
    }


    /// <summary>
    ///     Creates a new <see cref="Mat2" /> that contains subtraction of one matrix from another.
    /// </summary>
    /// <param name="matrix1">The first <see cref="Mat2" />.</param>
    /// <param name="matrix2">The second <see cref="Mat2" />.</param>
    /// <param name="result">The result of the matrix subtraction as an output parameter.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Subtract(ref Mat2 matrix1, ref Mat2 matrix2, out Mat2 result)
    {
        result.XAxis.X = matrix1.XAxis.X - matrix2.XAxis.X;
        result.XAxis.Y = matrix1.XAxis.Y - matrix2.XAxis.Y;

        result.YAxis.X = matrix1.YAxis.X - matrix2.YAxis.X;
        result.YAxis.Y = matrix1.YAxis.Y - matrix2.YAxis.Y;

    }


    /// <summary>
    ///     Swap the matrix rows and columns.
    /// </summary>
    /// <param name="matrix">The matrix for transposing operation.</param>
    /// <returns>The new <see cref="Mat2" /> which contains the transposing result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat2 Transpose(Mat2 matrix)
    {
        Mat2 ret;
        Transpose(ref matrix, out ret);
        return ret;
    }


    /// <summary>
    ///     Swap the matrix rows and columns.
    /// </summary>
    /// <param name="matrix">The matrix for transposing operation.</param>
    /// <param name="result">The new <see cref="Mat2" /> which contains the transposing result as an output parameter.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Transpose(ref Mat2 matrix, out Mat2 result)
    {
        Mat2 ret;
        ret.XAxis.X = matrix.XAxis.X;
        ret.XAxis.Y = matrix.YAxis.X;

        ret.YAxis.X = matrix.XAxis.Y;
        ret.YAxis.Y = matrix.YAxis.Y;
        result = ret;
    }

    #endregion


    #region public methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MultiplyRotation(float radians)
    {
        var rot = CreateRotation(radians);
        Multiply(ref this, ref rot, out this);
    }

    #endregion



    #region Object

    /// <summary>
    ///     Compares whether current instance is equal to specified <see cref="Mat2" /> without any tolerance.
    /// </summary>
    /// <param name="other">The <see cref="Mat2" /> to compare.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public bool Equals(Mat2 other) => this == other;


    /// <summary>
    ///     Compares whether current instance is equal to specified <see cref="Object" /> without any tolerance.
    /// </summary>
    /// <param name="obj">The <see cref="Object" /> to compare.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public override bool Equals(object obj)
    {
        if (obj is Mat2)
            return Equals((Mat2)obj);

        return false;
    }


    /// <summary>
    ///     Gets the hash code of this <see cref="Mat2" />.
    /// </summary>
    /// <returns>Hash code of this <see cref="Mat2" />.</returns>
    public override int GetHashCode() => XAxis.GetHashCode() + YAxis.GetHashCode();



    public override string ToString() => "{XAxis.X:" + XAxis.X + " XAxis.Y:" + XAxis.Y + "}"
                                         + " {YAxis.X:" + YAxis.X + " YAxis.Y:" + YAxis.Y + "}";

    #endregion

    public static Mat2 Create(float radians, Vector2 scale)
    {
        Mat2 result;
        Create(radians, scale, out result);
        return result;
    }

    public static void Create(float radians, Vector2 scale, out Mat2 result)
    {
        result.XAxis = new Vector2(MathF.Cos(radians), MathF.Sin(radians));
        result.XAxis.X = scale.X;
        result.YAxis.Y = scale.Y;
        result.XAxis.Y = 0;
        result.YAxis.X = 0;


        if (radians != 0)
        {
            result *= CreateRotation(radians);
        }
    }
}
