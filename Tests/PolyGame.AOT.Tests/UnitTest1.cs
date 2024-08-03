using PolyECS;
using PolyECS.Queries;
using PolyECS.Systems;

namespace PolyGame.AOT.Tests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestSimpleQuery()
    {
        using var world = new PolyWorld();
        world.Set(0);
        var sys = (TQuery<int> q) => {
            Assert.Equals(1, q.Query.Count());
            q.Query.Each(((ref int i) => i++));
        };
        world.RunSystemOnce(sys.IntoSystem());
        Assert.Equals(1, world.World.Get<int>());
        world.RunSystemOnce(sys.IntoSystem());
        Assert.Equals(2, world.World.Get<int>());
    }
    
    public struct C1
    {
        public float Value;
    }

    public class C2
    {
        public float Value;
    }
    
    [TestMethod]
    public void TestTupleQuery()
    {
        using var world = new PolyWorld();
        world.Entity().Set(new C1()).Set(new C2());
        var sys = (TQuery<(C1, C2)> q) => {
            Assert.Equals(1, q.Query.Count());
            q.Query.Each(((ref C1 c1, ref C2 c2) => {
                c1.Value++;
                c2.Value++;
            }));
        };
        world.RunSystemOnce(sys.IntoSystem());
        Assert.Equals(1, world.World.Get<int>());
        world.RunSystemOnce(sys.IntoSystem());
        Assert.Equals(2, world.World.Get<int>());
    }
}
