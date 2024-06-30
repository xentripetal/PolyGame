using TinyEcs;

namespace PolyGame.Tests;

public class WithoutRelationshipTest
{
    public record struct Flag { }
    [Fact]
    public void QueryWithoutParent()
    {
        using var world = new World();
        var parent = world.Entity("parent").Set<Flag>();
        var child = world.Entity("child").Set<Flag>();
        parent.AddChild(child);

        bool hadMatch = false;
        world.QueryBuilder().Without<(Defaults.ChildOf, Defaults.Wildcard)>().With<Flag>().Build().Each(entity => {
            hadMatch = true;
            Assert.Equal(parent.ID, entity.ID);
        });
        Assert.True(hadMatch, "Expected parent to be found in query result");
        
        hadMatch = false;
        world.QueryBuilder().With<(Defaults.ChildOf, Defaults.Wildcard)>().With<Flag>().Build().Each(entity => {
            hadMatch = true;
            Assert.Equal(child.ID, entity.ID);
        });
        Assert.True(hadMatch, "Expected child to be found in query result");
    }
}
