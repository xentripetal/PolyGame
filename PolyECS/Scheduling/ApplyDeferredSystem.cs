using Flecs.NET.Core;

namespace PolyECS.Systems;

/// <summary>
/// A special marker system that applies deferred buffers.
/// </summary>
public sealed class ApplyDeferredSystem : ASystem
{

    public ApplyDeferredSystem()
    {
        IsExclusive = true;
        HasDeferred = false;
    }
    private Access<UntypedComponent> access = new Access<UntypedComponent>().WriteAll();

    public override void Initialize(World scheduleWorld) { }
    
    public override void Run(World scheduleWorld)
    {
        scheduleWorld.DeferEnd();
        scheduleWorld.DeferBegin();
    }

    public override Access<UntypedComponent> GetAccess()
    {
        return access;
    }

    public override List<SystemSet> GetDefaultSystemSets() => throw new NotImplementedException();
    public override void ApplyDeferred(World scheduleWorld) { }
}
