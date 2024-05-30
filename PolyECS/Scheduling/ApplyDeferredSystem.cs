namespace PolyFlecs.Systems;

/// <summary>
/// A special marker system that applies deferred buffers.
/// </summary>
public sealed class ApplyDeferredSystem : System
{
    private Access access = new Access().WriteAll();

    public override void Initialize(IScheduleWorld scheduleWorld) { }

    public override void RunDeferred(IDeferredScheduleWorld scheduleWorld) { }

    public override void RunExclusive(IScheduleWorld world) { }

    public override Access GetAccess()
    {
        return access;
    }

    public override List<SystemSet> GetDefaultSystemSets() => throw new NotImplementedException();
    public override void ApplyDeferred(IScheduleWorld scheduleWorld) { }
}
