using Microsoft.Xna.Framework.Graphics;

namespace PolyGame.Graphics.Camera;

/// <summary>
///     wrapper for a RenderTarget2D that optionally takes care of resizing itself automatcially when the screen size
///     changes
/// </summary>
/// <remarks>Based on Nez.Textures.RenderTexture</remarks>
public class RenderTexture : IDisposable
{
    /// <summary>
    ///     handles what should happen when onSceneBackBufferSizeChanged. Defaults to SizeToSceneRenderTarget
    /// </summary>
    public enum RenderTextureResizeBehavior
    {
        None,
        SizeToSceneRenderTarget,
        SizeToScreen
    }

    /// <summary>
    ///     the RenderTarget2D this RenderTexture manages
    /// </summary>
    public RenderTarget2D RenderTarget;

    /// <summary>
    ///     resize behavior that should occur when onSceneBackBufferSizeChanged is called
    /// </summary>
    public RenderTextureResizeBehavior ResizeBehavior = RenderTextureResizeBehavior.SizeToSceneRenderTarget;


    #region constructors

    /// <summary>
    ///     helper for creating a full screen RenderTarget2D
    /// </summary>
    public RenderTexture(GraphicsDevice device, Screen screen) => RenderTarget = new RenderTarget2D(device, screen.Width, screen.Height, false,
        screen.PreferredBackBufferFormat, screen.PreferredDepthStencilFormat,
        0, RenderTargetUsage.PreserveContents);


    /**
    /// <summary>
    /// helper for creating a full screen RenderTarget2D with a specific DepthFormat
    /// </summary>
    /// <param name="preferredDepthFormat">Preferred depth format.</param>
    public RenderTexture(DepthFormat preferredDepthFormat)
    {
        RenderTarget = Textures.RenderTarget.Create(Screen.Width, Screen.Height, Screen.BackBufferFormat,
            preferredDepthFormat);
    }


    /// <summary>
    /// helper for creating a RenderTarget2D
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    public RenderTexture(int width, int height)
    {
        RenderTarget = Textures.RenderTarget.Create(width, height, Screen.BackBufferFormat,
            Screen.PreferredDepthStencilFormat);
    }


    /// <summary>
    /// helper for creating a RenderTarget2D
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="preferredDepthFormat">Preferred depth format.</param>
    public RenderTexture(int width, int height, DepthFormat preferredDepthFormat)
    {
        RenderTarget = Textures.RenderTarget.Create(width, height, Screen.BackBufferFormat, preferredDepthFormat);
    }


    /// <summary>
    /// helper for creating a RenderTarget2D
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="preferredFormat">Preferred format.</param>
    /// <param name="preferredDepthFormat">Preferred depth format.</param>
    public RenderTexture(int width, int height, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
    {
        RenderTarget = new RenderTarget2D(Core.GraphicsDevice, width, height, false, preferredFormat,
            preferredDepthFormat, 0, RenderTargetUsage.PreserveContents);
    }
    **/

    #endregion


    public void Dispose()
    {
        if (RenderTarget != null && !RenderTarget.IsDisposed)
        {
            RenderTarget.Dispose();
            RenderTarget = null;
        }
    }


    /// <summary>
    ///     called by Renderers automatically when appropriate. Lets the resizeBehavior kick in so auto resizing can occur
    /// </summary>
    /// <param name="newWidth">New width.</param>
    /// <param name="newHeight">New height.</param>
    public void OnSceneBackBufferSizeChanged(Screen screen, int newWidth, int newHeight)
    {
        switch (ResizeBehavior)
        {
            case RenderTextureResizeBehavior.None:
                break;

            case RenderTextureResizeBehavior.SizeToSceneRenderTarget:
                Resize(newWidth, newHeight);
                break;

            case RenderTextureResizeBehavior.SizeToScreen:
                Resize(screen.Width, screen.Height);
                break;
        }
    }


    /// <summary>
    ///     resizes the RenderTarget2D to match the back buffer size
    /// </summary>
    public void ResizeToFitBackbuffer(Screen screen)
    {
        Resize(screen.Width, screen.Height);
    }


    /// <summary>
    ///     resizes the RenderTarget2D to the specified size
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    public void Resize(int width, int height)
    {
        var device = RenderTarget.GraphicsDevice;
        var bufferFormat = RenderTarget.Format;

        // no need to resize if we are already the right size
        if (RenderTarget.Width == width && RenderTarget.Height == height && !RenderTarget.IsDisposed)
            return;

        // retain the same DepthFormat when we recreate the RenderTarget2D
        var depthFormat = RenderTarget.DepthStencilFormat;

        // unload if necessary
        Dispose();

        RenderTarget = new RenderTarget2D(device, width, height, false, bufferFormat, depthFormat,
            0, RenderTargetUsage.PreserveContents);
    }


    public static implicit operator RenderTarget2D(RenderTexture tex) => tex.RenderTarget;
}
