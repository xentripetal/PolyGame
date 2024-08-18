using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PolyGame.Graphics.Camera;

public record struct RenderTargetConfig
{
    public Color ClearColor;
    public RenderTarget2D Texture;
}
