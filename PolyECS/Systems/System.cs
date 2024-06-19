using TinyEcs;

namespace PolyECS.Systems;

public abstract class ASystem
{
    public abstract void Initialize(World world);
    public abstract void RunDeferred(World world);
    public abstract void RunExclusive(World world);

    /// <summary>
    /// Returns `true` if the system has deferred buffers
    /// </summary>
    public bool HasDeferred { get; protected set; }

    public bool IsExclusive { get; protected set; }

    public abstract Access<ComponentInfo> GetAccess();

    public abstract List<SystemSet> GetDefaultSystemSets();

    public virtual SystemSet ToSystemSet()
    {
        return new SystemTypeSet<ASystem>();
    }

    public abstract void ApplyDeferred(World scheduleWorld);
}
