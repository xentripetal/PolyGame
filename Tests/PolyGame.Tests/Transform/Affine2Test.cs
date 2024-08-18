using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PolyGame.Tests.Transform;

public class Affine2Test
{
    [Fact]
    public void KeepsState()
    {
        var affine = new Affine2(new Vector2(0, 1), MathF.PI / 2, new Vector2(2, 3));
        Assert.Equal(new Vector2(0, 1), affine.Translation);
        Assert.Equal(MathF.PI / 2, affine.RotationRadians);
        Assert.Equal(new Vector2(2, 3), affine.Scale);
    }

    [Fact]
    public void MulIdentity()
    {
        var affine = new Affine2(new Vector2(0, 1), MathF.PI / 2, new Vector2(2, 3)) * Affine2.Identity;
        Assert.Equal(new Vector2(0, 1), affine.Translation);
        Assert.Equal(MathF.PI / 2, affine.RotationRadians);
        Assert.Equal(new Vector2(2, 3), affine.Scale);
    }

    [Fact]
    public void Parent()
    {
        var parent = new Affine2(new Vector2(0, 1), 0, new Vector2(1, 1));
        var child = new Affine2(new Vector2(1, 0), 0, new Vector2(1, 1));
        var affine = parent * child;
        Assert.Equal(new Vector2(1, 1), affine.Translation);
        Assert.Equal(0, affine.RotationRadians);
        Assert.Equal(new Vector2(1, 1), affine.Scale);
    }

    [Fact]
    public void ParentScaled()
    {
        var parent = new Affine2(new Vector2(0, 1), 0, new Vector2(2, 2));
        var child = new Affine2(new Vector2(1, 0), 0, new Vector2(1, 1));
        var affine = parent * child;
        Assert.Equal(new Vector2(2, 1), affine.Translation);
        Assert.Equal(0, affine.RotationRadians);
        Assert.Equal(new Vector2(2, 2), affine.Scale);
    }


    [Fact]
    public void ParentRotated()
    {
        var parent = new Affine2(new Vector2(0, 1), MathF.PI, new Vector2(1, 1));
        var child = new Affine2(new Vector2(1, 0), 0, new Vector2(1, 1));
        var affine = parent * child;
        Assert.Equal(new Vector2(-1, 1), affine.Translation, new Vector2Comparer());
        Assert.Equal(MathF.PI, affine.RotationRadians, new RadianComparer());
        Assert.Equal(new Vector2(1, 1), affine.Scale);
    }

    [Fact]
    public void ParentRotatedScaled()
    {
        var parent = new Affine2(new Vector2(-1, 1), MathF.PI, new Vector2(2, 2));
        var child = new Affine2(new Vector2(1, 0), 0, new Vector2(1, 1));
        var affine = parent * child;
        Assert.Equal(new Vector2(-3, 1), affine.Translation, new Vector2Comparer());
        Assert.Equal(MathF.PI, affine.RotationRadians, new RadianComparer());
        Assert.Equal(new Vector2(2, 2), affine.Scale, new Vector2Comparer());
    }

    [Fact]
    public void MatrixParentRotatedScaled()
    {
        var parent = new Matrix2D(new Vector2(-1, 1), MathF.PI, new Vector2(2, 2));
        var child = new Matrix2D(new Vector2(1, 0), 0, new Vector2(1, 1));
        var affine = parent * child;
        //Assert.Equal(new Vector2(-3, 1), affine.Translation, new Vector2Comparer());
        // Fails
        Assert.Equal(MathF.PI, affine.Rotation, new RadianComparer());
        Assert.Equal(new Vector2(2, 2), affine.Scale, new Vector2Comparer());
    }

    [Fact]
    public void Matrix2DParent()
    {
        var parent = new Matrix2D(new Vector2(-1, 1), 0, new Vector2(1, 1));
        var child = new Matrix2D(new Vector2(1, 0), 0, new Vector2(1, 1));
        var affine = parent * child;
        Assert.Equal(new Vector2(0, 1), affine.Translation);
        Assert.Equal(0, affine.Rotation);
        Assert.Equal(new Vector2(1, 1), affine.Scale);
    }

    protected class Vector2Comparer : IEqualityComparer<Vector2>
    {
        public bool Equals(Vector2 x, Vector2 y) => MathF.Abs(x.X - y.X) < 0.0001f && MathF.Abs(x.Y - y.Y) < 0.0001f;
        public int GetHashCode(Vector2 obj) => obj.GetHashCode();
    }

    protected class RadianComparer : IEqualityComparer<float>
    {
        public bool Equals(float x, float y)
        {
            if (MathF.Abs(x - y) < 0.0001f)
            {
                return true;
            }
            return x > y ? 2 * MathF.PI - x + y < 0.0001f : 2 * MathF.PI - y + x < 0.0001f;
        }

        public int GetHashCode(float obj) => obj.GetHashCode();
    }
}
