using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyGame;
using PolyGame.Components.Transform;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Sprites;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
try
{
    using var verse = new Verse();
    verse.AddPluginBundle(new DefaultPlugins());
    // TODO hack for calling init
    verse.RunOneFrame();
    var assetServer = verse.World.GetResource<AssetServer>();
    for (int x = -100; x < 100; x++)
    {
        for (int y = -100; y < 100; y++)
        {
            var entity = verse.World.Entity($"Sprite{x}_{y}");
            var sb = new SpriteBundle
            {
                Texture = assetServer.Get().Load<Texture2D>("Content/Missing.png", false),
                Transform =
                {
                    Position = new Vector2(x, y)
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
            Zoom = 0,
            ZoomBounds = new Range<float>(1, 4)
        },
        Inset = new CameraInset(),
        Transform = new TransformBundle2D(),
    };
    cb.Apply(verse.World.Entity("TestCamera"));
    verse.Run();
}
catch (Exception e)
{
    Console.Error.Write(e.ToString());
}
Log.CloseAndFlush();
