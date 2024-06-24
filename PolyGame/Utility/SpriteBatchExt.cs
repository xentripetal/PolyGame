using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components.Render;

namespace PolyGame;

public static class SpriteBatchExt
{
    public static void Begin(
        this SpriteBatch spriteBatch,
        Material material,
        Matrix? matrix = null,
        SpriteSortMode sortMode = SpriteSortMode.Deferred,
        RasterizerState rasterizerState = null
    )
    {
        spriteBatch.Begin(sortMode, material.BlendState, material.SamplerState, material.DepthStencilState, rasterizerState, material.Effect, matrix);
    }

    public static void Begin(this SpriteBatch spriteBatch, Matrix transformMatrix)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Globals.DefaultSamplerState, DepthStencilState.None,
            RasterizerState.CullCounterClockwise, null, transformMatrix);
    }
}
