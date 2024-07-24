using System.Diagnostics;
using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Components.Render.Extract;

namespace PolyGame;

public partial class App : Game, IDisposable
{
    public PolyWorld RenderWorld;
    public Schedule RenderSchedule;
    public PolyWorld GameWorld;
    public Schedule GameSchedule;

    public AssetServer AssetServer;

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
        IsMouseVisible = false;
        IsFixedTimeStep = false;

        RenderWorld = new PolyWorld();
        RenderSchedule = new Schedule("render");
        GameWorld = new PolyWorld();
        GameSchedule = new Schedule("game");
        AssetServer = new AssetServer([GameWorld.World, RenderWorld.World]);
    }

    protected override void Initialize()
    {
        base.Initialize();
        RenderWorld.SetResource(new SpriteBatch(graphicsDevice: GraphicsDevice, capacity: 2048));
        ApplyPlugins();
    }

    /// <summary>
    /// Task for progressing the game world in the current frame
    /// </summary>
    protected Task GameWorldProgress = Task.CompletedTask;

    /// <summary>
    /// Task for rendering the game world from the previous frame
    /// </summary>
    protected Task RenderWorldProgress = Task.CompletedTask;


    public bool SynchronousRendering = false;

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        GameWorldProgress = ProgressSchedule(GameSchedule, GameWorld, gameTime.ElapsedGameTime);
    }

    protected virtual async Task ProgressSchedule(Schedule schedule, PolyWorld world, TimeSpan gameTime)
    {
        await Task.Run(() => schedule.Run(world));
    }


    protected TimeSpan previousFrameElapsedTime = TimeSpan.Zero;

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        if (SynchronousRendering)
        {
            GameWorldProgress.Wait();
            Extract();
            RenderWorldProgress = ProgressSchedule(RenderSchedule, RenderWorld, gameTime.ElapsedGameTime);
            RenderWorldProgress.Wait();
        }
        else
        {
            // We render the previous frame, so if this is the first frame, we won't render anything.
            // Otherwise we start rendering the previous frame and then wait for both this frames logic to finish
            // and the render before prepping the rendering for the future frame.
            if (_hasRenderState)
            {
                RenderWorldProgress = ProgressSchedule(RenderSchedule, RenderWorld, previousFrameElapsedTime);
            }
            Task.WaitAll(GameWorldProgress, RenderWorldProgress);
            previousFrameElapsedTime = gameTime.ElapsedGameTime;
            Extract();
        }
    }

    public List<IExtractor> Extractors = new ();
    private bool _hasRenderState;

    protected virtual void Extract()
    {
        RenderWorld.World.DeleteWith<DeleteAfterRender>();
        // Clear out last frames entities (except systems)
        foreach (var extractor in Extractors)
        {
            extractor.Extract(GameWorld.World, RenderWorld.World);
        }

        // There is now a render state in the render world
        _hasRenderState = true;
    }

    public void Dispose()
    {
        RenderWorld.Dispose();
        GameWorld.Dispose();
        base.Dispose();
    }
}

public interface IExtractor
{
    public void Extract(World sourceWorld, World targetWorld);
}
