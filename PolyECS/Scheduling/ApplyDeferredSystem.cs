using TinyEcs;

namespace PolyECS.Systems;

/// <summary>
/// A special marker system that applies deferred buffers.
/// </summary>
public sealed class ApplyDeferredSystem : ASystem
{
    private Access<ComponentInfo> access = new Access<ComponentInfo>().WriteAll();

    public override void Initialize(World scheduleWorld) { }

    public override void RunDeferred(World scheduleWorld) { }

    public override void RunExclusive(World world) { }

    public override Access<ComponentInfo> GetAccess()
    {
        return access;
    }

    public override List<SystemSet> GetDefaultSystemSets() => throw new NotImplementedException();
    public override void ApplyDeferred(World scheduleWorld) { }
}
