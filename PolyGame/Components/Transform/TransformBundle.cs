using Flecs.NET.Core;

namespace PolyGame.Components.Transform;

public record struct TransformBundle(Transform Transform, GlobalTransform GlobalTransform)
{
    public void Apply(Entity entity)
    {
        entity.Set(Transform);
        entity.Set(GlobalTransform);
    }
}
