using Flecs.NET.Core;

namespace PolyECS;

/// <summary>
///     Optional marker for a type to support a callback on registration for annotating type information.
/// </summary>
public interface IComponent
{
    public abstract static void Register(UntypedComponent component);
}
