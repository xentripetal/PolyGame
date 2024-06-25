using TinyEcs;

namespace PolyGame.Tests;

public class WithoutRelationshipTest
{
    [Fact]
    public void QueryWithoutParent()
    {
        using var world = new World();
        var parent = world.Entity("parent");
        var child = world.Entity("child");
        parent.AddChild(child);

        bool hadMatch = false;
        world.Query<Without<Defaults.ChildOf>>().Each(entity => {
            hadMatch = true;
            Assert.Equal(parent, entity);
        });
        Assert.True(hadMatch, "Expected parent to be found in query result");
    }
}
