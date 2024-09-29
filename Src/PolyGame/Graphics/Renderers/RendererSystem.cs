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

public partial class RendererSystem : AutoSystem
{
    [ParamProvider("cameras")]
    protected QueryParam BuildCamerasQuery(PolyWorld world) => new (world.QueryBuilder().With<ComputedCamera>().With<CameraRenderGraph>().With<RenderableList>()
        .With<RenderTargetConfig>().Optional()
        .Build());

    [AutoRunMethod]
    public void Run(
        Query cameras,
        ClearColor clearColor,
        GraphicsDevice device,
        Batcher batch,
        DrawFuncRegistry registry,
        AssetServer assets,
        FinalRenderTarget finalRenderTarget
    )
    {
        device.SetRenderTarget(finalRenderTarget.SceneRenderTarget);
        var hadCamera = false;
        cameras.Each((
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
            renderGraph.Graph.Render(assets, registry, ref cCam, batch, device, clearColor.Color, renderTexture, renderables);
            renderables.Clear();
        });
        if (!hadCamera)
        {
            Log.Warning("No Camera found in the world!");
        }
        device.SetRenderTarget(null);
        device.Clear(clearColor.Color);
        batch.Begin(BlendState.Opaque, Globals.DefaultSamplerState, null, null);
        batch.Draw(finalRenderTarget.SceneRenderTarget, finalRenderTarget.FinalRenderDestinationRect, Color.White);
        batch.End();
    }
}

public record struct ClearColor(Color Color) { };
public record struct CameraRenderGraph(RenderGraph Graph) { };
