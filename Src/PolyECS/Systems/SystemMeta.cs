using Flecs.NET.Core;

namespace PolyECS.Systems;

public class SystemMeta
{
    public FilteredAccessSet<ulong> ComponentAccessSet = new ();
    public Access<ulong> ResourceAccess = new ();
    public bool HasDeferred;
    public string Name;
    public Access<TableComponentId> TableComponentAccess = new ();

    public SystemMeta(string name)
    {
        Name = name;
    }
}
