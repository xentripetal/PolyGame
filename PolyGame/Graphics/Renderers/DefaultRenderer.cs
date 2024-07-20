using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components.Render;
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

    public override void Render(ref ComputedCamera cam, GraphicsDevice device, SpriteBatch batch, RenderableList renderables)
    {
        BeginRender(ref cam, device, batch);

        /**
        foreach (var renderable in renderables)
        {
            if (renderable.Enabled && renderable.IsVisibleFromCamera(cam))
                RenderAfterStateCheck(renderable, cam, batch);
        }
        **/

        if (ShouldDebugRender && Globals.DebugRenderEnabled)
            DebugRender(ref cam, device, batch, renderables);

        EndRender(batch);
    }
}