using System;
using Flecs.NET.Core;
using JetBrains.Annotations;
using PolyECS.Systems;

namespace PolyECS.Tests.Systems;

public class ParamTest
{
    private class CountUpSystem : ClassSystem<Ref<int>>
    {
        public CountUpSystem() : base("CountUpSystem") { }

        protected override ISystemParam<Ref<int>> CreateParam(PolyWorld world) => new SingletonParam<int>();

        public override void Run(Ref<int> param)
        {
            param.Get()++;
        }
    }

    private class SingleQuerySystem : ClassSystem<Query>
    {
        public SingleQuerySystem([NotNull] Query query, [NotNull] string name, Action<Query> cb) : base(name)
        {
            _cb = cb;
            _query = query;
        }
        private Query _query;

        private Action<Query> _cb;

        protected override ISystemParam<Query> CreateParam(PolyWorld world) => new QueryParam(_query);

        public override void Run(Query param)
        {
            _cb(param);
        }
    }

    [Fact]
    public void SingletonTest()
    {
        using var world = new PolyWorld();
        world.Set(0);
        world.RunSystemOnce(new CountUpSystem());
        Assert.Equal(1, world.World.Get<int>());
        world.RunSystemOnce(new CountUpSystem());
        Assert.Equal(2, world.World.Get<int>());
    }

    [Fact]
    public unsafe void SimpleQueryTest()
    {
        using var world = new PolyWorld();
        world.World.Entity().Set<int>(0);
        world.World.Entity().Set<int>(0).Set<double>(1);
        world.World.Entity().Set<int>(0).Set<double>(1).Set(1.0f);

        var query = world.World.QueryBuilder().With<int>().Out().Without<float>().With<double>().In().Optional().Build();
        var system = new SingleQuerySystem(query, "Query", (q) => {
            Assert.Equal(2, q.Count());
        });
        world.RunSystemOnce(system);
        system.GetAccess().Should().BeEquivalentTo(new Access<ulong>().AddWrite(Type<int>.Id(world.World)).AddRead(Type<double>.Id(world.World)));
        system.GetTableAccess().ReadsAndWrites.Count.Should().Be(2);
        system.GetTableAccess().Writes.Count.Should().Be(1);
    }
}
