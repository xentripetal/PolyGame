using System;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Xunit.Abstractions;

namespace PolyECS.Tests;

public class TQueryTest(WorldFixture world) : IClassFixture<WorldFixture>
{
    public PolyWorld World = world.World;

    [Fact]
    public void IndexedTerm()
    {
        var qIndex = new TQuery<int, In<Term0>>(World);
        Assert.Equal(flecs.ecs_inout_kind_t.EcsIn, qIndex.Query.Term(0).InOut());
    }

    [Fact]
    public void AllTermsSingle()
    {
        var qAllSingle = new TQuery<int, In<AllTerms>>(World);
        Assert.Equal(flecs.ecs_inout_kind_t.EcsIn, qAllSingle.Query.Term(0).InOut());
    }

    [Fact]
    public void AllTermsMulti()
    {
        var qAllMulti = new TQuery<int, float, bool, double, In<AllTerms>>(World);
        for (var i = 0; i < 4; i++)
            Assert.Equal(flecs.ecs_inout_kind_t.EcsIn, qAllMulti.Query.Term(i).InOut());
    }

    [Fact]
    public void NestedTuple()
    {
        var qNestedTuple = new TQuery<int, float, In<(Term0, Optional<Term1>)>>(World);
        Assert.Equal(flecs.ecs_inout_kind_t.EcsIn, qNestedTuple.Query.Term(0).InOut());
        Assert.Equal(flecs.ecs_inout_kind_t.EcsIn, qNestedTuple.Query.Term(1).InOut());
        Assert.Equal(flecs.ecs_oper_kind_t.EcsOptional, qNestedTuple.Query.Term(1).Oper());
    }

    [Fact]
    public unsafe void Traverse()
    {
        var qTraverse = new TQuery<int, float, (Cascade<Up<Optional<Term1>>>, Cached)>(World);
        var manualQTraverse = World.QueryBuilder<int, float>().TermAt(1).Optional().Parent().Cascade().Cached().Build();
        Assert.Equal(manualQTraverse.Term(1), qTraverse.Query.Term(1));
        Assert.Equal(flecs.ecs_query_cache_kind_t.EcsQueryCacheAll, qTraverse.Query.Handle->cache_kind);
    }


    [Fact]
    public unsafe void TestTraverseDuplicated()
    {
        var qRepeated = new TQuery<int, int, (In<AllTerms>, Cascade<Up<Optional<Term1>>>, Cached)>(World);
        var manualRepeated = World.QueryBuilder<int, int>().TermAt(0).In().TermAt(1).In().Optional().Parent().Cascade().Cached().Build();
        var t1 = manualRepeated.Term(1);
        var t2 = qRepeated.Query.Term(1);
        Assert.Equal(t1, t2);
        Assert.Equal(manualRepeated.Handle->cache_kind, qRepeated.Query.Handle->cache_kind);
    }
}