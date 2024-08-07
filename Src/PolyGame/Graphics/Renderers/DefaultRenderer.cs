using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components.Render;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Renderable;

namespace PolyGame.Graphics.Renderers;

public class DefaultRenderer : Renderer
{
    /// <summary>
    /// renders all renderLayers
    /// </summary>
    /// <param name="renderOrder">Render order.</param>
    /// <param name="camera">Camera.</param>
    public DefaultRenderer(int renderOrder = 0) : base(renderOrder)
    {
    }

    public override void Render(DrawFuncRegistry registry, ref ComputedCamera cam, GraphicsDevice device, Batcher batch, RenderableList renderables)
    {
        BeginRender(ref cam, device, batch);

        foreach (var layer in renderables.GetLayers())
        {
            foreach (var renderable in renderables.GetRenderables(layer))
            {
                registry.GetDrawFunc(renderable.DrawFuncIndex).Invoke(renderable, batch);
            }
        }

        if (ShouldDebugRender && Globals.DebugRenderEnabled)
            DebugRender(ref cam, device, batch, renderables);

        EndRender(batch);
    }
}