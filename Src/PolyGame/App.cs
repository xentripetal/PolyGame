using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Graphics;

namespace PolyGame;

public partial class App : Game, IDisposable
{
    public Schedule RenderSchedule;
    public PolyWorld World;
    public Schedule GameSchedule;
    public Texture2D? MissingTexture;
    public AssetServer Assets;

    public App(
        int width = 1280,
        int height = 720,
        bool isFullScreen = false,
        string windowTitle = "PolyGame",
        string contentDirectory = "Content",
        bool hardwareModeSwitch = true
    ) : base()
    {
        var graphicsManager = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = width,
            PreferredBackBufferHeight = height,
            IsFullScreen = isFullScreen,
            SynchronizeWithVerticalRetrace = true,
            HardwareModeSwitch = hardwareModeSwitch,
            PreferHalfPixelOffset = true,
        };
        Window.Title = windowTitle;

        graphicsManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

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

    protected override void Initialize()
    {
        base.Initialize();
        Assets.AddLoader(new ImageLoader(GraphicsDevice));
        World.SetResource(GraphicsDevice);
        World.SetResource(new SpriteBatch(graphicsDevice: GraphicsDevice, capacity: 2048));
        World.SetResource(new Batcher(graphicsDevice: GraphicsDevice));
        World.SetResource(GraphicsDevice.Viewport);
        ApplyPlugins();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        //World.World.Progress((float)gameTime.ElapsedGameTime.TotalSeconds);
        World.World.Progress();
        GameSchedule.Run(World);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        RenderSchedule.Run(World);
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
