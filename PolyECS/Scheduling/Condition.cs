using TinyEcs;

namespace PolyECS.Systems;

public interface Condition
{
    public void Initialize(World world);
    public bool Evaluate(World world);
}
