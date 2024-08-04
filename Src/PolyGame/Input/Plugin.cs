using Microsoft.Xna.Framework.Input;
using PolyECS.Scheduling.Configs;

namespace PolyGame.Input;

public enum InputSets
{
    CopyInput
}

public class InputPlugin : IPlugin
{
    public void Apply(App app)
    {
        app.World.RegisterResource<KeyboardState>();
        app.World.RegisterResource<PreviousKeyboardState>();
        app.World.RegisterResource<MouseState>();
        app.World.RegisterResource<PreviousMouseState>();
        app.AddSystems(SystemConfigs.Of(new PopulateKeyboardState(), new PopulateMouseState()).InSet(InputSets.CopyInput));
    }
}
