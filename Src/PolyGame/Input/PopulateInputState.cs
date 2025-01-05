using Microsoft.Xna.Framework.Input;
using PolyECS;
using PolyECS.Systems;

namespace PolyGame.Input;

public record struct PreviousKeyboardState(KeyboardState State) { }

public partial class PopulateKeyboardState : AutoSystem
{
    public void Run(ResMut<KeyboardState> state, ResMut<PreviousKeyboardState?> prevState)
    {
        prevState.Set(new PreviousKeyboardState(state.Value));
        state.Set(Keyboard.GetState());
    }
}

public record struct PreviousMouseState(MouseState State) { }

public partial class PopulateMouseState : AutoSystem
{
    public void Run(ResMut<MouseState> state, ResMut<PreviousMouseState?> prevState)
    {
        prevState.Set(new PreviousMouseState(state.Value));
        state.Set(Mouse.GetState());
    }
}
