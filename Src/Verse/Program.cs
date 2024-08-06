using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyGame;
using PolyGame.Components.Transform;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Sprites;
using Serilog;
using Verse;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
try
{
    using var app = new App();
    app.AddPluginBundle(new DefaultPlugins())
        .AddSystems(Schedules.Update, new MoveCameraSystem());
    // TODO hack for calling init
    app.RunOneFrame();
    var assetServer = app.World.GetResource<AssetServer>();
    for (int x = 0; x < 16; x++)
    {
        for (int y = 0; y < 16; y++)
        {
            var entity = app.World.Entity($"Sprite{x}_{y}");
            var sb = new SpriteBundle
            {
                Texture = assetServer.Get().Load<Texture2D>("Content/Missing.png", false),
                Transform =
                {
                    Position = new Vector2(x * 16, y * 16)
                }
            };
            sb.Apply(entity);
        }
    }
    var cb = new CameraBundle
    {
        Camera = new Camera
        {
            PositionZ3D = 0,
            NearClipPlane3D = 0,
            FarClipPlane3D = 0,
            RawZoom = 4,
            Origin = default,
            Zoom = 1,
            ZoomBounds = new Range<float>(1, 4)
        },
        Inset = new CameraInset(),
        Transform = new TransformBundle2D(),
    };
    cb.Apply(app.World.Entity("TestCamera"));
    app.Run();
}
catch (Exception e)
{
    Console.Error.Write(e.ToString());
}
Log.CloseAndFlush();
