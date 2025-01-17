using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using PolyGame.Graphics.Sprites;
using PolyGame.Transform;

namespace PolyGame.Graphics.Lights;

public abstract class BaseLightBundle
{
    public TransformBundle Transform;
    public ZIndex ZIndex;
    public RenderBounds Bounds;
    public uint Layer;

    /// <param name="sortLayer">Layer the light should be on. Required since this is defined in the cameras rendergraph</param>
    /// <param name="index"></param>
    /// <param name="transform"></param>
    public BaseLightBundle(uint sortLayer, ZIndex index = default, TransformBundle? transform = null)
    {
        Transform = transform ?? new TransformBundle();
        ZIndex = index;
        Layer = sortLayer;
    }

    protected abstract Entity ApplyLight(Entity entity);

    public Entity Apply(Entity entity)
    {
        return ApplyLight(Transform.Apply(entity)
            .Set(ZIndex)
            .Add<GlobalZIndex>()
            .Set(new SortLayer(Layer)));
    }
}

public class PointLightBundle : BaseLightBundle
{
    public PointLight Light;

    public PointLightBundle(uint sortLayer, PointLight? light = null, ZIndex index = default, TransformBundle? transform = null) : base(sortLayer, index,
        transform) => Light = light ?? new PointLight();

    protected override Entity ApplyLight(Entity entity)
    {
        return entity.Set(Light);
    }
}

public class SpotLightBundle : BaseLightBundle
{
    public SpotLight Light;

    public SpotLightBundle(uint sortLayer, SpotLight? light = null, ZIndex index = default, TransformBundle? transform = null) : base(sortLayer, index,
        transform)
    {
        Light = light ?? new SpotLight();
    }

    protected override Entity ApplyLight(Entity entity)
    {
        return entity.Set(Light);
    }
}
