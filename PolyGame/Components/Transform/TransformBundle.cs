
using TinyEcs;

namespace PolyGame.Components.Transform;

public record struct TransformBundle(Transform Transform, GlobalTransform GlobalTransform)
{
    public void Apply(EntityView entity)
    {
        entity.Set(Transform);
        entity.Set(GlobalTransform);
    }
}
