namespace PolyECS.Systems;

public class SystemMeta
{
    public FilteredAccessSet<ulong> ComponentAccessSet;

    // TODO - do we need this? Deferred is handled by the runner and the world
    public bool HasDeferred;
    public string Name;
    public Access<TableComponentId> TableComponentAccess;

    public SystemMeta(string name)
    {
        Name = name;
        ComponentAccessSet = new FilteredAccessSet<ulong>();
        TableComponentAccess = new Access<TableComponentId>();
        HasDeferred = false;
    }
}
