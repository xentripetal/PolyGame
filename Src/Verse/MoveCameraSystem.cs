using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Graphics.Camera;
using PolyGame.Transform;

namespace Verse;

public partial class MoveCameraSystem : AutoSystem
{
    [ParamProvider("cameras")]
    public QueryParam BuildCamerasQuery(PolyWorld world)
    {
        return Param.Of(world.QueryBuilder().With<Position2D>().InOut().With<Camera>().InOutNone().Build());
    }

    [AutoRunMethod]
    public void Run(Query cameras, KeyboardState state, GameTime gameTime)
    {
        var delta = gameTime.ElapsedGameTime.TotalSeconds;
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
