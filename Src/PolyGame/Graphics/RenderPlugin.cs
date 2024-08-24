using Microsoft.Xna.Framework;
using PolyECS.Scheduling.Configs;
using PolyGame.Graphics.Lights;
using PolyGame.Graphics.Renderable;
using PolyGame.Graphics.Renderers;
using PolyGame.Graphics.Sprites;

namespace PolyGame.Graphics;

public enum RenderSets
{
    PropagateZIndex,
    Queue,
    Sort,
    Render
}

public class RenderPlugin : IPlugin
{
    public void Apply(App app)
    {
        var registry = new DrawFuncRegistry();

        app.SetResource(registry)
            .SetResource(new FinalRenderTarget(app.GetResource<Screen>()))
            .SetResource(new RenderableList())
            .SetResource(new ClearColor(Color.CornflowerBlue))
            .ConfigureSets(Schedules.Render, SetConfigs.Of(RenderSets.PropagateZIndex, RenderSets.Queue, RenderSets.Sort, RenderSets.Render).Chained())
            .AddSystems(Schedules.Render,
                new PropagateZIndex().InSet(RenderSets.PropagateZIndex),
                new QueueSprites(registry).InSet(RenderSets.Queue),
                SystemConfigs.Of(new QueueAreaLights(), new QueueDirLights(), new QueuePointLights(), new QueueSpotLights()).InSet(RenderSets.Queue),
                new SortRenderablesSystem().InSet(RenderSets.Sort),
                new RendererSystem().InSet(RenderSets.Render)
            );
    }
}
