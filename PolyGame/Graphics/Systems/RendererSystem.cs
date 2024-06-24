using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components.Render;
using PolyGame.Graphics.Renderable;
using PolyGame.Graphics.Renderers;
using TinyEcs;

namespace PolyGame.Systems.Render;

public class RendererSystem
{
    protected Query Cameras;
    protected GraphicsDevice GraphicsDevice;
    protected SpriteBatch Batch;

    public RendererSystem(Scheduler scheduler)
    {
        scheduler.AddSystem((Query<(ComputedCamera, CameraRenderGraph, Optional<RenderTargetConfig>)> q, Res<ClearColor> clearColor, Res<RenderableList> renderables, Res<GraphicsDevice> device, Res<SpriteBatch> batch) => {
            Render(q, clearColor, renderables, device, batch);
        }, Stages.AfterUpdate);
    }

    protected void Render(Query<(ComputedCamera, CameraRenderGraph, Optional<RenderTargetConfig>)> Cameras, Res<ClearColor> clearColor, Res<RenderableList> renderables, Res<GraphicsDevice> device, Res<SpriteBatch> batch)
    {

        Cameras.Each((
            ref ComputedCamera cCam,
            ref CameraRenderGraph renderGraph,
            ref RenderTargetConfig renderTarget
        ) => {
            var hasRenderTexture = !Unsafe.IsNullRef(renderTarget);
            var renderTexture = hasRenderTexture ? renderTarget.Texture : null;
            if (renderables.Value == null)
            {
                return;
            }
            renderGraph.Graph.Render(ref cCam, Batch, GraphicsDevice, clearColor.Value.Color, renderTexture, renderables.Value);
        });
    }
}

public record struct ClearColor(Color Color) { };
public record struct CameraRenderGraph(RenderGraph Graph) { };
