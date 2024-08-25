using Flecs.NET.Core;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Graphics.Renderable;

namespace PolyGame.Graphics.Renderers;

public class SortRenderablesSystem : ClassSystem<Query>
{
    protected override ISystemParam<Query> CreateParam(PolyWorld world) => Param.Of(world.Query<RenderableList>());

    public override void Run(Query renderables)
    {
        renderables.Each((ref RenderableList list) => list.Sort());
    }
}
