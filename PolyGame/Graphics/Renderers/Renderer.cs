using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components.Render;
using PolyGame.Graphics.Renderable;
using PolyGame.Systems.Render;

namespace PolyGame.Graphics.Renderers;

public abstract class Renderer : IComparable<Renderer>
{
    public Renderer(int renderOrder = 0)
    {
        RenderOrder = renderOrder;
    }
    // Default material when no material is set on a renderable element
    public Material Material = Material.DefaultMaterial;

    /// <summary>
    /// Specifies the order in which the Renderer will be called in a RenderGraph
    /// </summary>
    public readonly int RenderOrder = 0;

    /// <summary>
    /// If not null this renderer will render into the RenderTarget instead of to the screen
    /// </summary>
    public RenderTexture? RenderTexture;

    /// <summary>
    /// if renderTarget is not null this Color will be used to clear the screen
    /// </summary>
    public Color RenderTargetClearColor = Color.Transparent;

    /// <summary>
    /// flag for this renderer that decides if it should debug render or not. The render method receives a bool (debugRenderEnabled)
    /// letting the renderer know if the global debug rendering is on/off. The renderer then uses the local bool to decide if it
    /// should debug render or not.
    /// </summary>
    public bool ShouldDebugRender = true;

    /// <summary>
    /// if true, the Scene will call SetRenderTarget with the scene RenderTarget. The default implementaiton returns true if the Renderer
    /// has a renderTexture
    /// </summary>
    /// <value><c>true</c> if wants to render to scene render target; otherwise, <c>false</c>.</value>
    public virtual bool WantsToRenderToGraphRenderTarget => RenderTexture == null;

    /// <summary>
    /// if true, the Scene will call the render method AFTER all PostProcessors have finished. This must be set to true BEFORE calling
    /// Scene.addRenderer to take effect and the Renderer should NOT have a renderTexture. The main reason for this type of Renderer
    /// is so that you can render your UI without post processing on top of the rest of your Scene. The ScreenSpaceRenderer is an
    /// example Renderer that sets this to true;
    /// </summary>
    public bool WantsToRenderAfterPostProcessors;

    /// <summary>
    /// holds the current Material of the last rendered Renderable (or the Renderer.material if no changes were made)
    /// </summary>
    protected Material? CurrentMaterial;

    public virtual void Unload() => RenderTexture?.Dispose();

    public virtual void OnAddedToGraph(RenderGraph graph) { }

    public virtual void BeginRender(ref ComputedCamera cam, GraphicsDevice device, SpriteBatch batch)
    {
        if (RenderTexture != null)
        {
            device.SetRenderTarget(RenderTexture);
            device.Clear(RenderTargetClearColor);
        }
        CurrentMaterial = Material;
        batch.Begin(CurrentMaterial, cam.TransformMatrix);
    }

    abstract public void Render(ref ComputedCamera cam, GraphicsDevice device, SpriteBatch batch, RenderableList renderables);

    /// <summary>
    /// renders the RenderableComponent flushing the Batcher and resetting current material if necessary
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void RenderAfterStateCheck(ComputedCamera cam, SpriteBatch batch)
    {
        /**
        // check for Material changes
        if (renderable.Material != null && renderable.Material != CurrentMaterial)
        {
            CurrentMaterial = renderable.Material;
            if (CurrentMaterial.Effect != null)
                CurrentMaterial.OnPreRender(cam);
            FlushBatch(cam, batch);
        }
        else if (renderable.Material == null && CurrentMaterial != Material)
        {
            CurrentMaterial = Material;
            FlushBatch(cam, batch);
        }

        renderable.Render(batch, cam);
        **/
    }

    void FlushBatch(ComputedCamera cam, SpriteBatch batch)
    {
        batch.End();
        batch.Begin(CurrentMaterial, cam.TransformMatrix);
    }

    /// ends the Batcher and clears the RenderTarget if it had a RenderTarget
    /// </summary>
    protected virtual void EndRender(SpriteBatch batch) => batch.End();

    /// <summary>
    /// default debugRender method just loops through all entities and calls entity.debugRender. Note that you are in the middle of a batch
    /// at this point so you may want to call Batcher.End and Batcher.begin to clear out any Materials and items awaiting rendering.
    /// </summary>
    protected virtual void DebugRender(ref ComputedCamera cam, GraphicsDevice device, SpriteBatch batch, RenderableList renderables)
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
    
    public virtual void OnSceneBackBufferSizeChanged(Screen screen, int newWidth, int newHeight) => RenderTexture?.OnSceneBackBufferSizeChanged(screen, newWidth, newHeight);

    public int CompareTo(Renderer other) => RenderOrder.CompareTo(other.RenderOrder);
}
