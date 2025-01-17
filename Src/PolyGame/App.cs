using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Scheduling;
using PolyGame.Assets;
using PolyGame.Graphics;

namespace PolyGame;

public partial class App : Game, IDisposable
{
    private bool _hasRenderState;
    protected GraphicsDeviceManager _manager;
    public AssetServer Assets;

    protected Batcher Batcher;

    public MainScheduleOrder MainScheduleOrder = new();
    public PolyWorld World;

    public App(
        int width = 1280,
        int height = 720,
        bool isFullScreen = false,
        string windowTitle = "PolyGame",
        string contentDirectory = "Content",
        bool hardwareModeSwitch = true
    )
    {
        _manager = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = width,
            PreferredBackBufferHeight = height,
            IsFullScreen = isFullScreen,
            SynchronizeWithVerticalRetrace = true,
            HardwareModeSwitch = hardwareModeSwitch,
            PreferHalfPixelOffset = true
        };
        Window.Title = windowTitle;
        Window.AllowUserResizing = true;

        _manager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

        Content.RootDirectory = contentDirectory;
        IsMouseVisible = true;
        IsFixedTimeStep = false;

        World = new PolyWorld();
        Assets = new AssetServer([World.FlecsWorld]);
        Assets.AddLoader(new XNBAssetLoader(Content));
        World.SetResource(Assets);
        World.SetResource(MainScheduleOrder);
        World.RegisterResource<GameTime>();

        var schedules = World.MustGetResource<ScheduleContainer>();
        foreach (var label in MainScheduleOrder.AllLabels)
        {
            schedules.Insert(new Schedule(label));
        }
    }

    public new void Dispose()
    {
        GC.SuppressFinalize(this);
        World.Dispose();
        Assets.Dispose();
        base.Dispose();
    }

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
        World.SetResource(gameTime);
        World.FlecsWorld.Set(gameTime);
        // Progress any internal flecs routines such as http server
        World.FlecsWorld.Progress();
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

    ~App()
    {
        Dispose();
    }
}