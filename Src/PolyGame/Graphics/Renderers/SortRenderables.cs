using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Renderable;
using Serilog;

namespace PolyGame.Graphics.Renderers;

public class SortRenderablesSystem : ClassSystem<Query>
{
    protected override ISystemParam<Query> CreateParam(PolyWorld world) => Param.Of(world.Query<RenderableList>());

    public override void Run(Query renderables)
    {
        renderables.Each((ref RenderableList list) => list.Sort());
    }
}
