using PolyGame;
using Serilog;
using Verse;
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
try
{
    using var app = new App();
    app.AddPluginBundle(new DefaultPlugins())
        .AddSystem<MoveCameraSystem>(Schedules.Update)
        .AddSystem<RotateSpriteSystem>(Schedules.Update)
        .AddSystem<SpawnTestSprites>(Schedules.Startup)
        .AddSystem<SpawnCamera>(Schedules.Startup);
    app.Run();
}
catch (Exception e)
{
    Console.Error.Write(e.ToString());
}
Log.CloseAndFlush();
