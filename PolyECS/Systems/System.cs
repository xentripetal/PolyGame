namespace PolyECS.Systems;

public abstract class System<T>
{
    public abstract void Initialize(IScheduleWorld world);
    public abstract void RunDeferred(IDeferredScheduleWorld world);
    public abstract void RunExclusive(IScheduleWorld world);

    /// <summary>
    /// Returns `true` if the system has deferred buffers
    /// </summary>
    public bool HasDeferred { get; protected set; }

    public bool IsExclusive { get; protected set; }

    public abstract Access<T> GetAccess();

    public abstract List<SystemSet> GetDefaultSystemSets();

    public virtual SystemSet ToSystemSet()
    {
        return new SystemTypeSet<System<T>, T>();
    }

    public abstract void ApplyDeferred(IScheduleWorld scheduleWorld);
}
