using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components.Render.Extract;
using PolyGame.Systems.Render;
using TinyEcs;

namespace PolyGame;

public class Core : Game
{
    public World RenderWorld;
    public Scheduler RenderSchedule;
    public World GameWorld;
    public Scheduler GameSchedule;

    public Core(
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
            // Nez uses half pixel offset but I don't plan on using FNA shaders so that should be fine?
            //PreferHalfPixelOffset = true
        };

        graphicsManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

        Content.RootDirectory = contentDirectory;
        IsMouseVisible = false;
        IsFixedTimeStep = false;

        RenderWorld = new World();
        RenderSchedule = new Scheduler(RenderWorld);
        GameWorld = new World();
        GameSchedule = new Scheduler(GameWorld);

        var renderer = new RendererSystem(RenderWorld, GraphicsDevice);
        var batch = new SpriteBatch(graphicsDevice: GraphicsDevice, capacity: 2048);
        RenderSchedule.AddResource(batch);
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
        GameWorldProgress = ProgressSchedule(GameSchedule, gameTime.ElapsedGameTime);
    }

    protected virtual async Task ProgressSchedule(Scheduler scheduler , TimeSpan gameTime)
    {
        float delta = gameTime.Milliseconds;
        await Task.Run(scheduler.Run);
    }


    protected TimeSpan previousFrameElapsedTime = TimeSpan.Zero;

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        if (SynchronousRendering)
        {
            GameWorldProgress.Wait();
            Extract();
            RenderWorldProgress = ProgressSchedule(RenderSchedule, gameTime.ElapsedGameTime);
            RenderWorldProgress.Wait();
        }
        else
        {
            // We render the previous frame, so if this is the first frame, we won't render anything.
            // Otherwise we start rendering the previous frame and then wait for both this frames logic to finish
            // and the render before prepping the rendering for the future frame.
            if (_hasRenderState)
            {

                RenderWorldProgress = ProgressSchedule(RenderSchedule, previousFrameElapsedTime);
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
        RenderWorld.QueryBuilder().With<DeleteAfterRender>().Build().Each(entity => entity.Delete());
        // Clear out last frames entities (except systems)
        foreach (var extractor in Extractors)
        {
            extractor.Extract(GameWorld, RenderWorld);
        }

        // There is now a render state in the render world
        _hasRenderState = true;
    }
}

public interface IExtractor
{
    public void Extract(World sourceWorld, World targetWorld);
}
