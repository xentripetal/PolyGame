using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components.Render;
using PolyGame.Graphics;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Renderable;
using PolyGame.Graphics.Renderers;
using Serilog;

namespace PolyGame.Systems.Render;

public class RenderGraph
{
    public RenderGraph(IEnumerable<Renderer> renderers)
    {
        foreach (var renderer in renderers)
            AddRenderer(renderer);
    }

    protected bool Active = true;

    // TODO should this be here?
    public Screen Screen;

    /// <summary>
    /// adds a Renderer to the scene
    /// </summary>
    /// <returns>The renderer.</returns>
    /// <param name="renderer">Renderer.</param>
    public T AddRenderer<T>(T renderer) where T : Renderer
    {
        if (renderer.WantsToRenderAfterPostProcessors)
        {
            AfterPostProcessorRenderers.Add(renderer);
            AfterPostProcessorRenderers.Sort();
        }
        else
        {
            Renderers.Add(renderer);
            Renderers.Sort();
        }


        renderer.OnAddedToGraph(this);

        // TODO handle addition of active graph
        // if we already began let the PostProcessor know what size our RenderTarget is
        //if (Active)
        //renderer.OnSceneBackBufferSizeChanged(Screen, _sceneRenderTarget.Width, _sceneRenderTarget.Height);

        return renderer;
    }

    protected List<Renderer> Renderers = new ();
    protected List<Renderer> AfterPostProcessorRenderers = new ();

    public void Render(DrawFuncRegistry registry, ref ComputedCamera cam, Batcher batch, GraphicsDevice device, Color clearColor, RenderTarget2D? target, RenderableList renderables)
    {
        if (Renderers.Count == 0)
        {
            Log.Error("There are no Renderers in the Scene!");
            return;
        }

        // Renderers should always have those that require a RenderTarget first. They clear themselves and set themselves as
        // the current RenderTarget when they render. If the first Renderer wants the sceneRenderTarget we set and clear it now.
        if (Renderers[0].WantsToRenderToGraphRenderTarget)
        {
            device.SetRenderTarget(target);
            device.Clear(clearColor);
        }


        var lastRendererHadRenderTarget = false;
        for (var i = 0; i < Renderers.Count(); i++)
        {
            // MonoGame follows the XNA implementation so it will clear the entire buffer if we change the render target even if null.
            // Because of that, we track when we are done with our RenderTargets and clear the scene at that time.
            if (lastRendererHadRenderTarget && Renderers[i].WantsToRenderToGraphRenderTarget)
            {
                device.SetRenderTarget(target);
                device.Clear(clearColor);

                // force a Camera matrix update to account for the new Viewport size
                // TODO verify we don't need to manipulate the ComputedCamera
                /*
                if (_renderers.Buffer[i].Camera != null)
                    _renderers.Buffer[i].Camera.ForceMatrixUpdate();
                Camera.ForceMatrixUpdate();
                **/
            }
            Renderers[i].Render(registry, ref cam, device, batch, renderables);
            lastRendererHadRenderTarget = Renderers[i].RenderTexture != null;
        }
    }
}
