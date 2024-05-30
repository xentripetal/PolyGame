using Friflo.Json.Fliox.Transform.Query.Ops;

namespace PolyFlecs.Systems;

public abstract class System
{
    public abstract void Initialize(IScheduleWorld world);
    public abstract void RunDeferred(IDeferredScheduleWorld world);
    public abstract void RunExclusive(IScheduleWorld world);

    /// <summary>
    /// Returns `true` if the system has deferred buffers
    /// </summary>
    public bool HasDeferred { get; protected set; }

    public bool IsExclusive { get; protected set; }

    public abstract Access GetAccess();

    public abstract List<SystemSet> GetDefaultSystemSets();

    public virtual SystemSet ToSystemSet()
    {
        return new SystemTypeSet<System>();
    }

    public abstract void ApplyDeferred(IScheduleWorld scheduleWorld);
}
