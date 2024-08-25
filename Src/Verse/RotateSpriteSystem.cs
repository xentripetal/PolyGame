using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Graphics.Sprites;
using PolyGame.Transform;

namespace Verse;

public class RotateSpriteSystem : ClassSystem<Query, Res<KeyboardState>, Res<GameTime>>
{
    protected override (ISystemParam<Query>, ISystemParam<Res<KeyboardState>>, ISystemParam<Res<GameTime>>) CreateParams(PolyWorld world) => (
        Param.Of(world.QueryBuilder().With<Rotation2D>().InOut().With<Scale2D>().InOut().With<Sprite>().InOutNone().Build()),
        Param.OfRes<KeyboardState>(),
        Param.OfRes<GameTime>()
    );

    public override void Run(Query sprites, Res<KeyboardState> kbState, Res<GameTime> gameTime)
    {
        var optState = kbState.TryGet();
        if (!optState.HasValue)
        {
            return;
        }
        var delta = gameTime.Get().ElapsedGameTime.TotalSeconds;
        var state = optState.Value;
        var rotChange = 0f;
        var scaleChange = 0f;
        if (state.IsKeyDown(Keys.Q))
        {
            rotChange -= 1;
        }
        if (state.IsKeyDown(Keys.E))
        {
            rotChange += 1;
        }
        if (state.IsKeyDown(Keys.Left))
        {
            scaleChange -= 1;
        }
        if (state.IsKeyDown(Keys.Right))
        {
            scaleChange += 1;
        }



        sprites.Each((ref Rotation2D rotation, ref Scale2D scale) => {
            rotation.Degrees += rotChange * 10 * (float)delta;
            scale.Value += scaleChange * new Vector2(0.5f, 0.5f) * (float)delta;
        });
    }
}
