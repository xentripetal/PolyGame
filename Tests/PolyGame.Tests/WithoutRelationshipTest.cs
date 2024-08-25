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

        var hadMatch = false;

    }

    [Fact]
    public void OptionalRefTest()
    {
        Assert.False(RuntimeHelpers.IsReferenceOrContainsReferences<Optional<int>>());
    }

    public record struct Flag { }
}
