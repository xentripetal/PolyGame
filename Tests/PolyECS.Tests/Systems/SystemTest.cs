using System;
using Flecs.NET.Core;
using JetBrains.Annotations;
using PolyECS.Systems;

namespace PolyECS.Tests.Systems;

public class ParamTest
{
    [Fact]
    public void SingletonTest()
    {
        using var world = new PolyWorld();
        world.SetResource(0);
        world.RunSystemOnce(new CountUpSystem());
        Assert.Equal(1, world.MustGetResource<int>());
        world.RunSystemOnce(new CountUpSystem());
        Assert.Equal(2, world.MustGetResource<int>());
    }

    [Fact]
    public unsafe void SimpleQueryTest()
    {
        using var world = new PolyWorld();
        world.FlecsWorld.Entity().Set(0);
        world.FlecsWorld.Entity().Set(0).Set<double>(1);
        world.FlecsWorld.Entity().Set(0).Set<double>(1).Set(1.0f);

        var query = world.FlecsWorld.QueryBuilder().With<int>().Out().Without<float>().With<double>().In().Optional().Build();
        var system = new SingleQuerySystem(query,  q => {
            Assert.Equal(2, q.Count());
        });
        world.RunSystemOnce(system);
        system.Meta.Access.CombinedAccess.Should().BeEquivalentTo(new Access<AccessElement>().AddWrite(AccessElement.OfComponent(Type<int>.Id(world.FlecsWorld))).AddRead(AccessElement.OfComponent(Type<double>.Id(world.FlecsWorld))));
        system.Meta.StorageAccess.ReadsAndWrites.Count.Should().Be(2);
        system.Meta.StorageAccess.Writes.Count.Should().Be(1);
    }

    private class CountUpSystem : ClassSystem
    {

        protected override void BuildParameters(ParamBuilder builder)
        {
            param = builder.ResMut<int>();
        }
        
        private ResMut<int> param;

        public override void Run(PolyWorld world)
        {
            param.Value++;
        }
    }

    private class SingleQuerySystem : ClassSystem
    {
        private readonly Action<Query> _cb;
        private readonly Query _query;

        public SingleQuerySystem(Query query, Action<Query> cb) 
        {
            _cb = cb;
            _query = query;
        }

        protected override void BuildParameters(ParamBuilder builder)
        {
            builder.With(new QueryParam(_query));
        }

        public override void Run(PolyWorld world)
        {
            _cb(_query);
        }
    }
}
