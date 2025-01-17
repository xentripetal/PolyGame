using Microsoft.Xna.Framework;
using PolyGame.Transform;

namespace PolyGame.Tests.Transform;

public class TransformPropagationTest : SystemTest
{
    public TransformPropagationTest()
    {
        Schedule.AddSystems(new PropagateTransform());
        World.FlecsWorld.Component<Vector2>().Member<float>("X").Member<float>("Y");
        World.Register<Rotation2D>();
        World.Register<Position2D>();
        World.Register<Scale2D>();
    }

    [Fact]
    public void Test()
    {
        var e = World.Entity().Set(new Position2D(new Vector2(2, 2)));
        Progress();
        var trans = e.Get<GlobalTransform2D>().Value;
        Assert.Equal(new Vector2(2, 2), trans.Translation);
        Assert.Equal(new Vector2(1, 1), trans.Scale);
        Assert.Equal(0, trans.RotationDegrees);
    }

    [Fact]
    public void NoRotation()
    {
        var e = World.Entity();
        new TransformBundle(new Vector2(2, 2), 0, Vector2.One).Apply(e);
        Progress();
        var trans = e.Get<GlobalTransform2D>().Value;
        Assert.Equal(new Vector2(2, 2), trans.Translation);
        Assert.Equal(new Vector2(1, 1), trans.Scale);
        Assert.Equal(0, trans.RotationDegrees);
    }

    [Fact]
    public void WithRotation()
    {

        var e = World.Entity();
        new TransformBundle(new Vector2(2, 2), 90, Vector2.One).Apply(e);
        Progress();
        var trans = e.Get<GlobalTransform2D>().Value;
        Assert.Equal(new Vector2(2, 2), trans.Translation);
        Assert.Equal(new Vector2(1, 1), trans.Scale);
        Assert.Equal(90, trans.RotationDegrees);
    }

    [Fact]
    public void Parent()
    {
        var parent = World.Entity();
        new TransformBundle(new Vector2(1, 1), 0, Vector2.One).Apply(parent);

        var child = World.Entity();
        new TransformBundle(new Vector2(2, 2), 0, Vector2.One).Apply(child);
        child.ChildOf(parent);

        Progress();
        var trans = child.Get<GlobalTransform2D>().Value;
        Assert.Equal(new Vector2(3, 3), trans.Translation);
        Assert.Equal(new Vector2(1, 1), trans.Scale);
    }
}
