using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = System.Numerics.Vector2;

namespace PolyGame.Graphics;

public interface IScreen
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Vector2 Size { get; }
    public Vector2 Center { get; }
    public Point Offset { get; }
    public GraphicsDevice GraphicsDevice { get; }
    public int PreferredBackBufferHeight { get; set; }
    public DisplayOrientation SupportedOrientations { get; set; }
    public DepthFormat PreferredDepthStencilFormat { get; set; }
    public SurfaceFormat BackBufferFormat { get; }

    public void SetSize(int width, int height);
}

/// <summary>
/// A wrapper around a screen that allows for virtual sizing and offsets. 
/// </summary>
public class VirtualScreen : IScreen
{
    public VirtualScreen(Screen screen)
    {
        _screen = screen;
        Width = screen.Width;
        Height = screen.Height;
    }

    protected Screen _screen;

    public int Width { get; set; }
    public int Height { get; set; }

    public Vector2 Size => new(Width, Height);

    public Vector2 Center => new((Width / 2) + Offset.X, (Height / 2) + Offset.Y);

    public Point Offset { get; set; }

    public GraphicsDevice GraphicsDevice
    {
        get => _screen.GraphicsDevice;
    }

    public int PreferredBackBufferHeight
    {
        get => _screen.PreferredBackBufferHeight;
        set => _screen.PreferredBackBufferHeight = value;
    }

    public DisplayOrientation SupportedOrientations
    {
        get => _screen.SupportedOrientations;
        set => _screen.SupportedOrientations = value;
    }

    public DepthFormat PreferredDepthStencilFormat
    {
        get => _screen.PreferredDepthStencilFormat;
        set => _screen.PreferredDepthStencilFormat = value;
    }

    public SurfaceFormat BackBufferFormat => _screen.BackBufferFormat;


    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
    }
}