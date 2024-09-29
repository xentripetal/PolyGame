using Flecs.NET.Core;
using PolyECS;
using PolyECS.Queries;
using PolyECS.Systems;
using PolyGame.Graphics.Renderable;

namespace PolyGame.Graphics.Renderers;

public partial class SortRenderablesSystem : AutoSystem
{
    [AutoRunMethod]
    public void Run(TQuery<RenderableList> renderables)
    {
        renderables.Query.Each((ref RenderableList list) => list.Sort());
    }
}
