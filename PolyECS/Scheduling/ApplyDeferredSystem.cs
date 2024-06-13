namespace PolyECS.Systems;

/// <summary>
/// A special marker system that applies deferred buffers.
/// </summary>
public sealed class ApplyDeferredSystem<T> : System<T>
{
    private Access<T> access = new Access<T>().WriteAll();

    public override void Initialize(IScheduleWorld scheduleWorld) { }

    public override void RunDeferred(IDeferredScheduleWorld scheduleWorld) { }

    public override void RunExclusive(IScheduleWorld world) { }

    public override Access<T> GetAccess()
    {
        return access;
    }

    public override List<SystemSet> GetDefaultSystemSets() => throw new NotImplementedException();
    public override void ApplyDeferred(IScheduleWorld scheduleWorld) { }
}
