using Flecs.NET.Core;

namespace PolyECS;

public interface IComponent
{
    public static abstract void Register(UntypedComponent component);
}
