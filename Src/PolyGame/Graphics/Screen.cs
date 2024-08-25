using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PolyGame.Graphics;

public class Screen
{
    protected GraphicsDeviceManager _graphicsManager;

    public Screen(GraphicsDeviceManager manager) => _graphicsManager = manager;

    public GraphicsDevice GraphicsDevice => _graphicsManager.GraphicsDevice;
    /// <summary>
    ///     width of the GraphicsDevice back buffer
    /// </summary>
    /// <value>The width.</value>
    public int Width
    {
        get => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferWidth;
        set => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferWidth = value;
    }

    /// <summary>
    ///     height of the GraphicsDevice back buffer
    /// </summary>
    /// <value>The height.</value>
    public int Height
    {
        get => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferHeight;
        set => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferHeight = value;
    }

    /// <summary>
    ///     gets the Screen's size as a Vector2
    /// </summary>
    /// <value>The screen size.</value>
    public Vector2 Size => new (Width, Height);

    /// <summary>
    ///     gets the Screen's center.null Note that this is the center of the backbuffer! If you are rendering to a smaller
    ///     RenderTarget
    ///     you will need to scale this value appropriately.
    /// </summary>
    /// <value>The center.</value>
    public Vector2 Center => new (Width / 2, Height / 2);

    public int PreferredBackBufferWidth
    {
        get => _graphicsManager.PreferredBackBufferWidth;
        set => _graphicsManager.PreferredBackBufferWidth = value;
    }

    public int PreferredBackBufferHeight
    {
        get => _graphicsManager.PreferredBackBufferHeight;
        set => _graphicsManager.PreferredBackBufferHeight = value;
    }

    public int MonitorWidth => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;

    public int MonitorHeight => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

    public SurfaceFormat BackBufferFormat =>
        _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferFormat;

    public SurfaceFormat PreferredBackBufferFormat
    {
        get => _graphicsManager.PreferredBackBufferFormat;
        set => _graphicsManager.PreferredBackBufferFormat = value;
    }

    public bool SynchronizeWithVerticalRetrace
    {
        get => _graphicsManager.SynchronizeWithVerticalRetrace;
        set => _graphicsManager.SynchronizeWithVerticalRetrace = value;
    }

    // defaults to Depth24Stencil8
    public DepthFormat PreferredDepthStencilFormat
    {
        get => _graphicsManager.PreferredDepthStencilFormat;
        set => _graphicsManager.PreferredDepthStencilFormat = value;
    }

    public bool IsFullscreen
    {
        get => _graphicsManager.IsFullScreen;
        set => _graphicsManager.IsFullScreen = value;
    }

    public bool HardwareModeSwitch
    {
        get => _graphicsManager.HardwareModeSwitch;
        set => _graphicsManager.HardwareModeSwitch = value;
    }

    public DisplayOrientation SupportedOrientations
    {
        get => _graphicsManager.SupportedOrientations;
        set => _graphicsManager.SupportedOrientations = value;
    }

    public void ApplyChanges() => _graphicsManager.ApplyChanges();

    /// <summary>
    ///     sets the preferredBackBuffer then applies the changes
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    public void SetSize(int width, int height)
    {
        PreferredBackBufferWidth = width;
        PreferredBackBufferHeight = height;
        ApplyChanges();
    }
}
