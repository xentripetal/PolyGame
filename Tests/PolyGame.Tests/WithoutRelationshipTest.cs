using System.Runtime.CompilerServices;
using DotNext;
using Flecs.NET.Core;

namespace PolyGame.Tests;

public class WithoutRelationshipTest
{
    [Fact]
    public void QueryWithoutParent()
    {
        using var world = World.Create();
        var parent = world.Entity("parent").Add<Flag>();
        var child = world.Entity("child").Add<Flag>();
        child.ChildOf(parent);

        var count = 0;
        world.QueryBuilder().With<Flag>().Without(Ecs.ChildOf, Ecs.Wildcard).Build().Each((entity =>
        {
            Assert.Equal(entity, parent);
            count++;
        }));
        Assert.Equal(1, count);
    }

    [Fact]
    public void OptionalRefTest()
    {
        Assert.False(RuntimeHelpers.IsReferenceOrContainsReferences<Optional<int>>());
    }

    public record struct Flag { }
}
