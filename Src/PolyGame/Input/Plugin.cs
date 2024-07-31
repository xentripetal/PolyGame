using Microsoft.Xna.Framework.Input;
using PolyECS.Systems.Configs;

namespace PolyGame.Input;

public class InputPlugin : IPlugin
{
    public void Apply(App app)
    {
        app.World.RegisterResource<KeyboardState>();
        app.World.RegisterResource<PreviousKeyboardState>();
        app.World.RegisterResource<MouseState>();
        app.World.RegisterResource<PreviousMouseState>();
        app.GameSchedule.AddSystems(SystemConfigs.Of([new PopulateKeyboardState(), new PopulateMouseState()]));
    }
}
