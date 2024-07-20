using System.Runtime.CompilerServices;
using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Components.Render;
using PolyGame.Graphics.Renderable;
using PolyGame.Graphics.Renderers;

namespace PolyGame.Systems.Render;

public class RendererSystem : ClassSystem<Query, Res<ClearColor>, Res<RenderableList>, ResMut<GraphicsDevice>, ResMut<SpriteBatch>>
{
    protected Query Cameras;
    protected GraphicsDevice GraphicsDevice;
    protected SpriteBatch Batch;

    protected override (ISystemParam<Query>, ISystemParam<Res<ClearColor>>, ISystemParam<Res<RenderableList>>, ISystemParam<ResMut<GraphicsDevice>>,
        ISystemParam<ResMut<SpriteBatch>>) CreateParams(PolyWorld world)
    {
        return (
            world.World.Query<ComputedCamera, CameraRenderGraph, RenderTargetConfig>().AsParam(),
            new ResParam<ClearColor>(),
            new ResParam<RenderableList>(),
            new ResMutParam<GraphicsDevice>(),
            new ResMutParam<SpriteBatch>()
        );
    }

    public override void Run(
        Query Cameras,
        Res<ClearColor> clearColor,
        Res<RenderableList> renderables,
        ResMut<GraphicsDevice> graphicsDevice,
        ResMut<SpriteBatch> batch
    )
    {
        Cameras.Each((
            ref ComputedCamera cCam,
            ref CameraRenderGraph renderGraph,
            ref RenderTargetConfig renderTarget
        ) => {
            var hasRenderTexture = !Unsafe.IsNullRef(renderTarget);
            var renderTexture = hasRenderTexture ? renderTarget.Texture : null;
            if (renderables.IsEmpty)
            {
                return;
            }
            renderGraph.Graph.Render(ref cCam, Batch, GraphicsDevice, clearColor.Get().Color, renderTexture, renderables.Get());
        });
    }
}

public record struct ClearColor(Color Color) { };
public record struct CameraRenderGraph(RenderGraph Graph) { };
