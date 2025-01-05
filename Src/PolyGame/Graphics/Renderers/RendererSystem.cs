using System.Runtime.CompilerServices;
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
    public void Run(
        TQuery<ComputedCamera, CameraRenderGraph, RenderableList, (In<AllTerms>, InOut<Term2>,
            Optional<With<RenderTargetConfig>>)> cameras,
        [In] ClearColor clearColor, GraphicsDevice device, Batcher batcher,
        [In] DrawFuncRegistry registry, [In] AssetServer assets, FinalRenderTarget renderTarget)
    {
        device.SetRenderTarget(renderTarget.SceneRenderTarget);
        var hadCamera = false;
        cameras.Each((
            Entity en,
            ref ComputedCamera cCam,
            ref CameraRenderGraph renderGraph,
            ref RenderableList renderables
        ) =>
        {
            hadCamera = true;
            RenderTarget2D renderTexture = renderTarget.SceneRenderTarget;
            if (en.Has<RenderTargetConfig>())
                renderTexture = en.Get<RenderTargetConfig>().Texture;

            renderGraph.Graph.Render(assets, registry, ref cCam, batcher, device, clearColor.Color, renderTexture,
                renderables);
            renderables.Clear();
        });
        if (!hadCamera)
        {
            Log.Warning("No Camera found in the world!");
        }

        device.SetRenderTarget(null);
        device.Clear(clearColor.Color);
        batcher.Begin(BlendState.Opaque, Globals.DefaultSamplerState, null, null);
        batcher.Draw(renderTarget.SceneRenderTarget, renderTarget.FinalRenderDestinationRect, Color.White);
        batcher.End();
    }
}

public record struct ClearColor(Color Color)
{ };

public record struct CameraRenderGraph(RenderGraph Graph)
{ };