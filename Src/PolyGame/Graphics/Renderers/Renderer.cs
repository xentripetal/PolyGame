using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Assets;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Materials;
using PolyGame.Graphics.Renderable;

namespace PolyGame.Graphics.Renderers;

public abstract class Renderer : IComparable<Renderer>
{
    /// <summary>
    ///     Specifies the order in which the Renderer will be called in a RenderGraph
    /// </summary>
    public readonly int RenderOrder;

    /// <summary>
    ///     holds the current Material of the last rendered Renderable (or the Renderer.material if no changes were made)
    /// </summary>
    protected Material? CurrentMaterial;
    // Default material when no material is set on a renderable element
    public Material Material = Material.DefaultMaterial;

    /// <summary>
    ///     if renderTarget is not null this Color will be used to clear the screen
    /// </summary>
    public Color RenderTargetClearColor = Color.Transparent;

    /// <summary>
    ///     If not null this renderer will render into the RenderTarget instead of to the screen
    /// </summary>
    public RenderTexture? RenderTexture;

    /// <summary>
    ///     flag for this renderer that decides if it should debug render or not. The render method receives a bool
    ///     (debugRenderEnabled)
    ///     letting the renderer know if the global debug rendering is on/off. The renderer then uses the local bool to decide
    ///     if it
    ///     should debug render or not.
    /// </summary>
    public bool ShouldDebugRender = true;

    /// <summary>
    ///     if true, the Scene will call the render method AFTER all PostProcessors have finished. This must be set to true
    ///     BEFORE calling
    ///     Scene.addRenderer to take effect and the Renderer should NOT have a renderTexture. The main reason for this type of
    ///     Renderer
    ///     is so that you can render your UI without post processing on top of the rest of your Scene. The ScreenSpaceRenderer
    ///     is an
    ///     example Renderer that sets this to true;
    /// </summary>
    public bool WantsToRenderAfterPostProcessors;

    public Renderer(int renderOrder = 0) => RenderOrder = renderOrder;

    /// <summary>
    ///     if true, the Scene will call SetRenderTarget with the scene RenderTarget. The default implementaiton returns true
    ///     if the Renderer
    ///     has a renderTexture
    /// </summary>
    /// <value><c>true</c> if wants to render to scene render target; otherwise, <c>false</c>.</value>
    public virtual bool WantsToRenderToGraphRenderTarget => RenderTexture == null;

    public int CompareTo(Renderer other) => RenderOrder.CompareTo(other.RenderOrder);

    public virtual void Unload() => RenderTexture?.Dispose();

    public virtual void OnAddedToGraph(RenderGraph graph) { }

    public virtual void BeginRender(ref ComputedCamera cam, GraphicsDevice device, Batcher batch)
    {
        if (RenderTexture != null)
        {
            device.SetRenderTarget(RenderTexture);
            device.Clear(RenderTargetClearColor);
        }
        CurrentMaterial = Material;
        batch.Begin(CurrentMaterial, cam.TransformMatrix);
    }

    public abstract void Render(AssetServer assets, DrawFuncRegistry registry, ref ComputedCamera cam, GraphicsDevice device, Batcher batch, RenderableList renderables);

    /// <summary>
    ///     renders the RenderableComponent flushing the Batcher and resetting current material if necessary
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void RenderAfterStateCheck(DrawFuncRegistry registry, ComputedCamera cam, Batcher batch, AssetServer assets, RenderableReference renderable)
    {
        var mat = Material;
        if (renderable.Entity.Has<Material>())
        {
            mat = renderable.Entity.Get<Material>();
        }
        if (mat != CurrentMaterial)
        {
            CurrentMaterial = mat;
            batch.End();
            batch.Begin(CurrentMaterial, cam.TransformMatrix);
        }
        registry.GetDrawFunc(renderable.DrawFuncIndex).Invoke(assets, renderable, batch);
    }

    private void FlushBatch(ComputedCamera cam, Batcher batch)
    {
        batch.End();
        batch.Begin(CurrentMaterial, cam.TransformMatrix);
    }

    /// ends the Batcher and clears the RenderTarget if it had a RenderTarget
    /// </summary>
    protected virtual void EndRender(Batcher batch) => batch.End();

    /// <summary>
    ///     default debugRender method just loops through all entities and calls entity.debugRender. Note that you are in the
    ///     middle of a batch
    ///     at this point so you may want to call Batcher.End and Batcher.begin to clear out any Materials and items awaiting
    ///     rendering.
    /// </summary>
    protected virtual void DebugRender(ref ComputedCamera cam, GraphicsDevice device, Batcher batch, RenderableList renderables)
    {
        batch.End();
        batch.Begin(cam.TransformMatrix);

        /**
        foreach (var entity in renderables)
        {
            if (entity.Enabled)
                entity.DebugRender(batch);
        }
        **/
    }

    public virtual void OnSceneBackBufferSizeChanged(Screen screen, int newWidth, int newHeight)
        => RenderTexture?.OnSceneBackBufferSizeChanged(screen, newWidth, newHeight);
}
