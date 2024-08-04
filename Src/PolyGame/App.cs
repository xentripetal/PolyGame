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
    public PolyWorld World;
    public AssetServer Assets;
    protected GraphicsDeviceManager _manager;
    public MainScheduleOrder MainScheduleOrder = new ();

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

        World = new PolyWorld();
        Assets = new AssetServer([World.World]);
        Assets.AddLoader(new XNBAssetLoader(Content));
        World.SetResource(Assets);
        World.SetResource(MainScheduleOrder);

        var schedules = World.GetResource<ScheduleContainer>().TryGet().OrThrow(() => new ApplicationException("ScheduleContainer resource not found"));
        foreach (var label in MainScheduleOrder.AllLabels)
        {
            schedules.Insert(new Schedule(label));
        }
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

        // Run any startup systems
        foreach (var startupScheduleLabel in MainScheduleOrder.StartupLabels)
        {
            World.RunSchedule(startupScheduleLabel);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        // Progress any internal flecs routines such as http server
        World.World.Progress();
        // Run our main schedule
        foreach (var schedule in MainScheduleOrder.UpdateLabels)
        {
            World.RunSchedule(schedule);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        foreach (var schedule in MainScheduleOrder.RenderLabels)
        {
            World.RunSchedule(schedule);
        }
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
