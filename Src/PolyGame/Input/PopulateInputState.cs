using Flecs.NET.Core;
using Microsoft.Xna.Framework.Input;
using PolyECS;
using PolyECS.Systems;

namespace PolyGame.Input;

public record struct PreviousKeyboardState(KeyboardState State) { }

public class PopulateKeyboardState: ClassSystem<ResMut<KeyboardState>, ResMut<PreviousKeyboardState>>
{
    protected override (ISystemParam<ResMut<KeyboardState>>, ISystemParam<ResMut<PreviousKeyboardState>>) CreateParams(PolyWorld world) => (
        Param.OfResMut<KeyboardState>(),
        Param.OfResMut<PreviousKeyboardState>()
    );

    public override void Run(ResMut<KeyboardState> state, ResMut<PreviousKeyboardState> prevState)
    {
        if (state.HasValue)
        {
            var prevKeyboardState = prevState.GetRef().Get();
            prevKeyboardState.State = state.Get();
        }

        state.GetRef().Get() = Keyboard.GetState();
    }
}

public record struct PreviousMouseState(MouseState State) { }

public class PopulateMouseState: ClassSystem<ResMut<MouseState>, ResMut<PreviousMouseState>>
{
    protected override (ISystemParam<ResMut<MouseState>>, ISystemParam<ResMut<PreviousMouseState>>) CreateParams(PolyWorld world) => (
        Param.OfResMut<MouseState>(),
        Param.OfResMut<PreviousMouseState>()
    );

    public override void Run(ResMut<MouseState> state, ResMut<PreviousMouseState> prevState)
    {
        if (state.HasValue)
        {
            var prevMouseState = prevState.GetRef().Get();
            prevMouseState.State = state.Get();
        }

        state.GetRef().Get() = Mouse.GetState();
    }
}

