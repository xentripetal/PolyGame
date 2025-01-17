using Flecs.NET.Core;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Graphics.Renderable;

namespace PolyGame.Graphics.Renderers;

public class SortRenderablesSystem : ClassSystem
{
    private Query<RenderableList> _renderables;

    protected override void BuildParameters(ParamBuilder builder)
    {
        _renderables = builder.QueryBuilder<RenderableList>(qb => qb.TermAt(0).InOut());
    }

    public override void Run(PolyWorld world)
    {
        _renderables.Each((ref RenderableList list) => list.Sort());
    }
}
