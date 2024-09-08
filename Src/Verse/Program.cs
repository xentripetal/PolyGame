using PolyGame;
using PolyGame.Editor;
using PolyGame.ImGuiNet;
using PolyGame.Myra;
using Serilog;
using Verse;
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
try
{
    using var app = new App();
    app.AddPluginBundle(new DefaultPlugins())
        //.AddPlugin(new MyraPlugin())
        .AddPlugin(new ImGuiPlugin())
        .AddPlugin(new EditorPlugin())
        .AddSystem<MoveCameraSystem>(Schedules.Update)
        .AddSystem<RotateSpriteSystem>(Schedules.Update)
        .AddSystem<SpawnTestSprites>(Schedules.Startup)
        .AddSystem<SpawnCamera>(Schedules.Startup);
        //.AddSystem<SpawnGuiTest>(Schedules.Startup);
    app.Run();
}
catch (Exception e)
{
    Console.Error.Write(e.ToString());
}
Log.CloseAndFlush();
