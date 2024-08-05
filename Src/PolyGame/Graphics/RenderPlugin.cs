using Microsoft.Xna.Framework;
using PolyECS.Scheduling.Configs;
using PolyGame.Graphics.Renderable;
using PolyGame.Graphics.Sprites;

namespace PolyGame.Systems.Render;

public class RenderPlugin : IPlugin
{
    public void Apply(App app)
    {
        var registry = new DrawFuncRegistry();

        app.SetResource(registry)
            .SetResource(new RenderableList())
            .SetResource(new ClearColor(Color.CornflowerBlue))
            .ConfigureSets(Schedules.Render, SetConfigs.Of(RenderSets.Queue, RenderSets.Render).Chained())
            .AddSystems(Schedules.Render, new QueueSprites(registry).InSet(RenderSets.Queue), new RendererSystem().InSet(RenderSets.Render));
    }
}
