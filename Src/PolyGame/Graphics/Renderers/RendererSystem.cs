using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Components.Render;
using PolyGame.Graphics;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Renderable;
using Serilog;

namespace PolyGame.Systems.Render;

public class RendererSystem : ClassSystem<Query, Res<ClearColor>, ResMut<GraphicsDevice>, ResMut<Batcher>, ResMut<DrawFuncRegistry>>
{
    protected override (ISystemParam<Query>, ISystemParam<Res<ClearColor>>, ISystemParam<ResMut<GraphicsDevice>>,
        ISystemParam<ResMut<Batcher>>, ISystemParam<ResMut<DrawFuncRegistry>>) CreateParams(PolyWorld world)
    {
        return (
            Param.Of(world.World.QueryBuilder().With<ComputedCamera>().With<CameraRenderGraph>().With<RenderableList>().With<RenderTargetConfig>().Optional()
                .Build()),
            Param.OfRes<ClearColor>(),
            Param.OfResMut<GraphicsDevice>(),
            Param.OfResMut<Batcher>(),
            Param.OfResMut<DrawFuncRegistry>()
        );
    }

    public override void Run(
        Query Cameras,
        Res<ClearColor> clearColor,
        ResMut<GraphicsDevice> graphicsDevice,
        ResMut<Batcher> batch,
        ResMut<DrawFuncRegistry> registry
    )
    {
        var hadCamera = false;
        Cameras.Each((
            Entity en,
            ref ComputedCamera cCam,
            ref CameraRenderGraph renderGraph,
            ref RenderableList renderables
        ) => {
            hadCamera = true;
            RenderTarget2D renderTexture = null;
            if (en.Has<RenderTargetConfig>())
            {
                renderTexture = en.Get<RenderTargetConfig>().Texture;
            }
            renderGraph.Graph.Render(registry, ref cCam, batch, graphicsDevice, clearColor.Get().Color, renderTexture, renderables);
            renderables.Clear();
        });
        if (!hadCamera)
        {
            Log.Warning("No Camera found in the world!");
        }
    }
}

public record struct ClearColor(Color Color) { };
public record struct CameraRenderGraph(RenderGraph Graph) { };
