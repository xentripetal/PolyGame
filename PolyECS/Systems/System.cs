using Flecs.NET.Core;

namespace PolyECS.Systems;

public abstract class ASystem
{
    public abstract void Initialize(World world);
    public abstract void Run(World world);

    /// <summary>
    /// Returns `true` if the system has deferred buffers
    /// </summary>
    public bool HasDeferred { get; protected set; } = true;

    public bool IsExclusive { get; protected set; } = false;

    public abstract Access<UntypedComponent> GetAccess();

    public abstract List<SystemSet> GetDefaultSystemSets();

    public virtual SystemSet ToSystemSet()
    {
        return new SystemTypeSet(this);
    }

    public abstract void ApplyDeferred(World scheduleWorld);
}
