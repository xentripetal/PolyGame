using Microsoft.Xna.Framework.Input;
using PolyECS;
using PolyECS.Systems;

namespace PolyGame.Input;

public record struct PreviousKeyboardState(KeyboardState State) { }

public partial class PopulateKeyboardState : AutoSystem
{
    [AutoRunMethod]
    public void Run(ref KeyboardState state, PreviousKeyboardState prevState)
    {
        prevState.State = state;
        state = Keyboard.GetState();
    }
}

public record struct PreviousMouseState(MouseState State) { }

public partial class PopulateMouseState : AutoSystem
{
    [AutoRunMethod]
    public void Run(ref MouseState state, PreviousMouseState prevState)
    {
        prevState.State = state;
        state = Mouse.GetState();
    }
}
