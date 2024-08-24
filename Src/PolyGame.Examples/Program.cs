using PolyGame.Examples.Samples;
using Serilog;
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    DeferredLighting.Run();
}
catch (Exception e)
{
    Log.Error(e, "Error in DeferredLighting");
}
