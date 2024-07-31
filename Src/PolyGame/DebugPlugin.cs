using Flecs.NET.Bindings;
using Flecs.NET.Core;

namespace PolyGame;

public class DebugPlugin : IPlugin
{
    public void Apply(App app)
    {
        app.World.World.Import<Ecs.Stats>();
        app.World.World.Set(default(flecs.EcsRest));
    }
}
