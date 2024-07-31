using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Components.Render;
using PolyGame.Graphics;

namespace PolyGame;

public partial class App : Game, IDisposable
{
    public Schedule RenderSchedule;
    public PolyWorld World;
    public Schedule GameSchedule;
    public Texture2D? MissingTexture;
    public AssetServer Assets;

    protected GraphicsDeviceManager _manager;

    public App(
        int width = 1280,
        int height = 720,
        bool isFullScreen = false,
        string windowTitle = "PolyGame",
        string contentDirectory = "Content",
        bool hardwareModeSwitch = true
    ) : base()
    {
        _manager = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = width,
            PreferredBackBufferHeight = height,
            IsFullScreen = isFullScreen,
            SynchronizeWithVerticalRetrace = true,
            HardwareModeSwitch = hardwareModeSwitch,
            PreferHalfPixelOffset = true,
        };
        Window.Title = windowTitle;

        _manager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

        Content.RootDirectory = contentDirectory;
        IsMouseVisible = true;
        IsFixedTimeStep = false;

        RenderSchedule = new Schedule("render");
        World = new PolyWorld();
        GameSchedule = new Schedule("game");
        Assets = new AssetServer([World.World]);
        Assets.AddLoader(new XNBAssetLoader(Content));
        World.SetResource(Assets);
    }

    protected Batcher Batcher;

    protected override void Initialize()
    {
        base.Initialize();
        Assets.AddLoader(new ImageLoader(GraphicsDevice));
        World.SetResource(GraphicsDevice);
        Batcher = new Batcher(GraphicsDevice);
        // We don't use the SpriteBatch but still set it up in case it's needed
        World.SetResource(new SpriteBatch(GraphicsDevice, 2048));
        World.SetResource(Batcher);
        var screen = new Screen(_manager);
        screen.SetSize(1280, 720);
        World.SetResource(screen);
        World.SetResource(_manager.GraphicsDevice.Viewport);
        ApplyPlugins();

    }

    protected Texture2D _testTex;

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        //World.World.Progress((float)gameTime.ElapsedGameTime.TotalSeconds);
        World.World.Progress();
        GameSchedule.Run(World);
    }

    protected override void Draw(GameTime gameTime)
    {
        RenderSchedule.Run(World);
        base.Draw(gameTime);
    }

    protected List<IExtractor> Extractors = new ();
    private bool _hasRenderState;

    ~App()
    {
        Dispose();
    }

    public new void Dispose()
    {
        GC.SuppressFinalize(this);
        World.Dispose();
        Assets.Dispose();
        base.Dispose();
    }
}

public interface IExtractor
{
    public void Extract(World sourceWorld, World targetWorld);
}
