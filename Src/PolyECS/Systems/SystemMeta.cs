namespace PolyECS.Systems;

public class SystemMeta
{
    public bool HasDeferred;
    public bool IsExclusive;
    public string Name;
    public Access<StorageKey> StorageAccess;
    public FilteredAccessSet<AccessElement> Access;

    public SystemMeta(string name)
    {
        Name = name;
        Access = new FilteredAccessSet<AccessElement>();
        StorageAccess = new Access<StorageKey>();
        HasDeferred = true; // Due to the differences in Flecs vs bevy, we can't really determine if a system will push
                            // deferred changes or not. So we default to true and only disable it when we're certain.
    }
}
