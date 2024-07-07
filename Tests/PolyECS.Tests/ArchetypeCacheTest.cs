using System;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using Flecs.NET.Core;

namespace PolyECS.Tests;

public class TableCacheTest
{
    [Fact]
    public unsafe void TestComponentEquality()
    {
        using var world = World.Create();
        var pos = world.Component<Components.Position>();
        world.Entity().Add<Components.Relationship, Components.Position>();

        var pair = new Id(world, Type<Components.Relationship>.Id(world), Type<Components.Position>.Id(world));
        Assert.Equal(pair.TypeId(), pos.Entity);

        var wildcardPair = new Id(world, Ecs.Wildcard, Type<Components.Position>.Id(world));
        Assert.Equal(wildcardPair.TypeId(), pos.Entity);
    }

    /// <summary>
    /// Simple test for verifying my understanding of Flecs.NET's query behavior
    /// </summary>
    [Fact]
    public void TestRelationshipQueryBehavior()
    {
        unsafe
        {
            using var world = World.Create();
            world.Entity().Add<Components.Relationship, Components.Position>();
            var posId = Type<Components.Position>.Id(world);
            
            Assert.NotEqual(0, world.QueryBuilder().With<Components.Relationship, Components.Position>().Build().Count());
            Assert.NotEqual(0, world.QueryBuilder().With(Ecs.Wildcard, posId).Build().Count());
            Assert.Equal(0, world.QueryBuilder().With(posId, Ecs.Wildcard).Build().Count());
            Assert.Equal(0, world.Query<Components.Position>().Count());
        }
    }

    [Fact]
    public void TestUpdate()
    {
        unsafe
        {
            var world = World.Create();
            var cache = new TableCache(world);
            
            var pos = world.Component<Components.Position>();
            var vel = world.Component<Components.Velocity>();
            var rel = world.Component<Components.Relationship>();
            var relPos = new Id(world.Handle, rel, pos);
        
            world.Entity().Add<Components.Position>().Add<Components.Velocity>();
            world.Entity().Add<Components.Relationship, Components.Position>();
            cache.Update();
            
            Assert.Single(cache.TablesForType(vel.Id));
            Assert.Single(cache.TablesForType(pos.Id));
            Assert.Single(cache.TablesForType(relPos));
            Assert.Empty(cache.TablesForType(rel));
            Assert.Single(cache.TablesForType(new Id(world.Handle, Ecs.Wildcard, pos)));
            Assert.Single(cache.TablesForType(new Id(world.Handle, rel, Ecs.Wildcard)));

            world.Entity().Add<Components.Position>();
            cache.Update();
            Assert.Equal(2, cache.TablesForType(pos.Id).Count());
        }
    }
}
