using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Graphics.Sprites;
using PolyGame.Transform;

namespace Verse;

public partial class RotateSpriteSystem : AutoSystem
{
    public void Run(Query<Rotation2D, Scale2D> sprites, KeyboardState state, GameTime gameTime)
    {
        var delta = gameTime.ElapsedGameTime.TotalSeconds;
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


        sprites.Each((ref Rotation2D rotation, ref Scale2D scale) =>
        {
            rotation.Degrees += rotChange * 10 * (float)delta;
            scale.Value += scaleChange * new Vector2(0.5f, 0.5f) * (float)delta;
        });
    }
}
