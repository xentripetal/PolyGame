using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Assets;
using PolyGame.Graphics.Materials;
using PolyGame.Transform;

namespace PolyGame.Graphics.Sprites;

public struct Sprite
{
    public Color Color = Color.White;
    public SpriteEffects Effects = SpriteEffects.None;

    /// <summary>
    ///     <see cref="Anchor" /> point of the sprite in the world
    /// </summary>
    public Anchor Anchor = Anchor.Center;

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

    public static Anchor Center => new(0.5f, 0.5f);
    public static Anchor TopLeft => new(0.0f, 0.0f);
    public static Anchor TopRight => new(1.0f, 0.0f);
    public static Anchor BottomLeft => new(0.0f, 1.0f);
    public static Anchor BottomRight => new(1.0f, 1.0f);
    public static implicit operator Vector2(Anchor anchor) => new(anchor.X, anchor.Y);
}

public record struct ZIndex(int Value, bool Relative = false)
{
    public static implicit operator int(ZIndex z) => z.Value;
}

public record struct GlobalZIndex(int Value)
{
    public static implicit operator int(GlobalZIndex z) => z.Value;
}

public record struct SortLayer(uint Value)
{
    public static implicit operator uint(SortLayer z) => z.Value;
}

public class SpriteBundle
{
    public Sprite Sprite = new();
    public Texture2D Texture;
    public TransformBundle Transform = new();
    public Material? Material = null;
    public ZIndex ZIndex = new();
    public SortLayer Layer = new();

    public SpriteBundle WithMaterial(Material material)
    {
        Material = material;
        return this;
    }

    public SpriteBundle WithLayer(uint layer)
    {
        Layer = new SortLayer(layer);
        return this;
    }

    public SpriteBundle WithZIndex(int index)
    {
        ZIndex = new ZIndex(index);
        return this;
    }

    public SpriteBundle WithTransform(TransformBundle transform)
    {
        Transform = transform;
        return this;
    }

    public SpriteBundle WithSprite(Sprite sprite)
    {
        Sprite = sprite;
        return this;
    }


    public SpriteBundle(Texture2D texture)
    {
        Texture = texture;
    }

    public Entity Apply(Entity entity)
    {
        Transform.Apply(entity);
        if (Material != null)
        {
            entity.Set(Material);
        }
        return entity.Set(Sprite)
            .Set(Texture)
            .Set(ZIndex)
            .Set(Layer)
            .Add<RenderBounds>()
            .Add<GlobalZIndex>();
    }
}
