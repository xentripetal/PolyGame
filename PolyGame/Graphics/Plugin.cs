using PolyGame.Graphics.Renderable;
using TinyEcs;

namespace PolyGame.Systems.Render;

public class RenderPlugin : IPlugin
{
    public void Build(Scheduler scheduler)
    {
        scheduler.AddResource(new RenderableList());
        scheduler.AddResource(new ClearColor(Microsoft.Xna.Framework.Color.CornflowerBlue));
        RendererSystem renderer = new (scheduler);
    }
}
