using Microsoft.Xna.Framework.Graphics;
using PolyGame.Graphics.Sprites;
using PolyGame.Systems.Render;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
try
{
    using var verse = new Verse();
    verse.AddPlugin(new RenderPlugin());
    var sb = new SpriteBundle
    {
        Texture = verse.AssetServer.Load<Texture2D>("Missing"), 
    };
    sb.Apply(verse.GameWorld.Entity("TestSprite"));
    verse.Run();
}
catch (Exception e)
{
    Console.Error.Write(e.ToString());
}
Log.CloseAndFlush();
