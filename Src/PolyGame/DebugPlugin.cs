using Flecs.NET.Bindings;
using Flecs.NET.Core;

namespace PolyGame;

public class DebugPlugin : IPlugin
{
    public void Apply(App app)
    {
        app.World.FlecsWorld.Import<Ecs.Stats>();
        app.World.FlecsWorld.Set(default(flecs.EcsRest));
    }
}
