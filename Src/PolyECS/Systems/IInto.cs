using System.Runtime.CompilerServices;

namespace PolyECS.Systems;

public interface IIntoCondition
{
    public BaseSystem<bool> IntoCondition(PolyWorld world);
}

public interface IIntoSystem
{
    public BaseSystem<Empty> IntoSystem(PolyWorld world);
}

