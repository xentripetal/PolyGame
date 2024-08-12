using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components.Transform;
using PolyGame.Transform;

namespace PolyGame.Graphics.Sprites;

public struct Sprite
{
    public Color Color = Color.White;
    public SpriteEffects Effects = SpriteEffects.None;
    // TODO
    // - An optional custom size for the sprite that will be used when rendering, instead of the sizeof the sprite's image
    // - An optional rectangle representing the region of the sprite's image to render, instead of rendering
    //   the full image. This is an easy one-off alternative to using a [`TextureAtlas`](crate::TextureAtlas).
    //
    //   When used with a [`TextureAtlas`](crate::TextureAtlas), the rect
    //   is offset by the atlas's minimal (top-left) corner position.
    // [`Anchor`] point of the sprite in the world
    public Anchor Anchor;

    public Sprite() { }
}

public struct Anchor
{
    public float X;
    public float Y;

    public Anchor(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static Anchor Center => new Anchor(0.5f, 0.5f);
    public static Anchor TopLeft => new Anchor(0.0f, 0.0f);
    public static Anchor TopRight => new Anchor(1.0f, 0.0f);
    public static Anchor BottomLeft => new Anchor(0.0f, 1.0f);
    public static Anchor BottomRight => new Anchor(1.0f, 1.0f);
    public static implicit operator Vector2(Anchor anchor) => new Vector2(anchor.X, anchor.Y);
}

public class SpriteBundle
{
    public Sprite Sprite = new Sprite();
    public Handle<Texture2D> Texture = default;
    public TransformBundle Transform = new TransformBundle();

    public void Apply(Entity entity)
    {
        Transform.Apply(entity);
        entity.Set(Sprite)
            .Set(Texture);
    }
}
