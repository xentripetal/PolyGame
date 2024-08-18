using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Graphics.Camera;
using PolyGame.Transform;

namespace Verse;

public class MoveCameraSystem : ClassSystem<Query, Res<KeyboardState>, Res<GameTime>>
{
    protected override (ISystemParam<Query>, ISystemParam<Res<KeyboardState>>, ISystemParam<Res<GameTime>>) CreateParams(PolyWorld world) => (
        Param.Of(world.QueryBuilder().With<Position2D>().InOut().With<Camera>().InOutNone().Build()),
        Param.OfRes<KeyboardState>(),
        Param.OfRes<GameTime>()
    );

    public override void Run(Query cameras, Res<KeyboardState> kbState, Res<GameTime> gameTime)
    {
        var optState = kbState.TryGet();
        if (!optState.HasValue)
        {
            return;
        }
        var delta = gameTime.Get().ElapsedGameTime.TotalSeconds;
        var state = optState.Value;
        var move = Vector2.Zero;
        if (state.IsKeyDown(Keys.W))
        {
            move.Y -= 1;
        }
        if (state.IsKeyDown(Keys.S))
        {
            move.Y += 1;
        }
        if (state.IsKeyDown(Keys.A))
        {
            move.X -= 1;
        }
        if (state.IsKeyDown(Keys.D))
        {
            move.X += 1;
        }

        cameras.Each((ref Position2D pos) => {
            pos.Value += move * 100 * (float)delta;
        });
    }
}
