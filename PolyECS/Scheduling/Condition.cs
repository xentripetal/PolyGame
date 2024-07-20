using Flecs.NET.Core;

namespace PolyECS.Systems;

public interface Condition
{
    public void Initialize(PolyWorld world);
    public bool Evaluate(PolyWorld world);
}
