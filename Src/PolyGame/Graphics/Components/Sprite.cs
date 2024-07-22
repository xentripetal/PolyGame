using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PolyGame.Graphics.Components;

public struct Sprite
{
    public Color Color;
    public SpriteEffects Effects;
    // TODO
    // - An optional custom size for the sprite that will be used when rendering, instead of the sizeof the sprite's image
    // - An optional rectangle representing the region of the sprite's image to render, instead of rendering
    //   the full image. This is an easy one-off alternative to using a [`TextureAtlas`](crate::TextureAtlas).
    //
    //   When used with a [`TextureAtlas`](crate::TextureAtlas), the rect
    //   is offset by the atlas's minimal (top-left) corner position.
    // [`Anchor`] point of the sprite in the world
}
