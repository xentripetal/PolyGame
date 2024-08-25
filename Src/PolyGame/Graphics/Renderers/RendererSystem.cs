using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Assets;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Renderable;
using Serilog;

namespace PolyGame.Graphics.Renderers;

public class RendererSystem : ClassSystem<Query, Res<ClearColor>, ResMut<GraphicsDevice>, ResMut<Batcher>, ResMut<DrawFuncRegistry>, Res<AssetServer>, ResMut<FinalRenderTarget>>
{
    protected override (ISystemParam<Query>, ISystemParam<Res<ClearColor>>, ISystemParam<ResMut<GraphicsDevice>>,
        ISystemParam<ResMut<Batcher>>, ISystemParam<ResMut<DrawFuncRegistry>>, ISystemParam<Res<AssetServer>>, ISystemParam<ResMut<FinalRenderTarget>>) CreateParams(PolyWorld world)
        => (
            Param.Of(world.World.QueryBuilder().With<ComputedCamera>().With<CameraRenderGraph>().With<RenderableList>().With<RenderTargetConfig>().Optional()
                .Build()),
            Param.OfRes<ClearColor>(),
            Param.OfResMut<GraphicsDevice>(),
            Param.OfResMut<Batcher>(),
            Param.OfResMut<DrawFuncRegistry>(),
            Param.OfRes<AssetServer>(),
            Param.OfResMut<FinalRenderTarget>()
        );

    public override void Run(
        Query Cameras,
        Res<ClearColor> clearColor,
        ResMut<GraphicsDevice> graphicsDevice,
        ResMut<Batcher> batch,
        ResMut<DrawFuncRegistry> registry,
        Res<AssetServer> assets,
        ResMut<FinalRenderTarget> finalRenderTargetRes
    )
    {
        if (finalRenderTargetRes.IsEmpty)
        {
            Log.Error("No Final Render Target found in the world!");
            return;
        }
        var finalRenderTarget = finalRenderTargetRes.Get();
        var device = graphicsDevice.Get();
        device.SetRenderTarget(finalRenderTarget.SceneRenderTarget);
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
            else
            {
                renderTexture = finalRenderTarget.SceneRenderTarget;
            }
            renderGraph.Graph.Render(assets, registry, ref cCam, batch, graphicsDevice, clearColor.Get().Color, renderTexture, renderables);
            renderables.Clear();
        });
        if (!hadCamera)
        {
            Log.Warning("No Camera found in the world!");
        }
        var batcher = batch.Get();
        
        device.SetRenderTarget(null);
        device.Clear(clearColor.Get().Color);
        batcher.Begin(BlendState.Opaque, Globals.DefaultSamplerState, null, null);
        batcher.Draw(finalRenderTarget.SceneRenderTarget, finalRenderTarget.FinalRenderDestinationRect, Color.White);
        batcher.End();
    }
}

public record struct ClearColor(Color Color) { };
public record struct CameraRenderGraph(RenderGraph Graph) { };
