using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PolyGame.Components.Render;

public record struct RenderTargetConfig
{
    public Color ClearColor;
    public RenderTarget2D Texture;
}
