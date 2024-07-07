using Flecs.NET.Bindings;
using Flecs.NET.Core;

namespace PolyECS.Systems;

public class SystemMeta
{
    public string Name;
    public FilteredAccessSet<UntypedComponent> ComponentAccessSet;
    public Access<TableComponentId> TableComponentAccess;

    // TODO - do we need this? Deferred is handled by the runner and the world
    public bool HasDeferred;

    public SystemMeta(string name)
    {
        Name = name;
        ComponentAccessSet = new FilteredAccessSet<UntypedComponent>();
        TableComponentAccess = new Access<TableComponentId>();
        HasDeferred = false;
    }
}
