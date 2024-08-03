using PolyECS.Queries;
using PolyECS.Systems;

namespace PolyECS.Tests.Systems;

using static ActionSystem;

public class LambdaTest
{
    [Fact]
    public void SingletonTest()
    {
        using var world = new PolyWorld();
        world.Set(0);
        var sys = (ResMut<int> i) => {
            i.GetRef().Get()++;
        };
        world.RunSystemOnce(sys.IntoSystem());
        Assert.Equal(1, world.World.Get<int>());
        world.RunSystemOnce(sys.IntoSystem());
        Assert.Equal(2, world.World.Get<int>());
    }

    [Fact]
    public void QueryTest()
    {
        using var world = new PolyWorld();
        world.Set(0);
        var sys = (TQuery<int> q) => {
            Assert.Equal(1, q.Query.Count());
            q.Query.Each(((ref int i) => i++));
        };
        world.RunSystemOnce(sys.IntoSystem());
        Assert.Equal(1, world.World.Get<int>());
        world.RunSystemOnce(sys.IntoSystem());
        Assert.Equal(2, world.World.Get<int>());
    }
}
