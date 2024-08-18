using PolyGame.Examples.Samples;
using Serilog;
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

DeferredLighting.Run();
