using Flecs.NET.Core;
using PolyECS.Scheduling.Graph;

namespace PolyECS.Systems;

/// <summary>
/// A special marker system that applies deferred buffers.
/// </summary>
public sealed class ApplyDeferredSystem : RunSystem
{
    private Access<ulong> access = new Access<ulong>().WriteAll();
    private Access<TableComponentId> tableAccess = new Access<TableComponentId>().WriteAll();


    public override void Initialize(PolyWorld world) { }

    public override Empty Run(Empty _, PolyWorld world)
    {
        return Empty.Instance;
    }

    public override bool HasDeferred
    {
        get => false;
    }
    public override bool IsExclusive
    {
        get => true;
    }

    public override Access<ulong> GetAccess() => access;

    public override Access<TableComponentId> GetTableAccess() => tableAccess;

    public override List<ISystemSet> GetDefaultSystemSets()
    {
        return new List<ISystemSet>([new SystemReferenceSet(this)]);
    }

    public override void UpdateTableComponentAccess(TableCache cache) { }
}
