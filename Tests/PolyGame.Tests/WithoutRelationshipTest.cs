using Flecs.NET.Core;

namespace PolyGame.Tests;

public class WithoutRelationshipTest
{
    public record struct Flag { }
    [Fact]
    public void QueryWithoutParent()
    {
        using var world = World.Create();
        var parent = world.Entity("parent").Add<Flag>();
        var child = world.Entity("child").Add<Flag>();
        child.ChildOf(parent);

        bool hadMatch = false;

    }
}
