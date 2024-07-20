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

    public override object? Run(object? i, PolyWorld world)
    {
        return null;
    }

    public bool HasDeferred
    {
        get => false;
    }
    public bool IsExclusive
    {
        get => true;
    }

    public override Access<ulong> GetAccess() => access;

    public override Access<TableComponentId> GetTableAccess() => tableAccess;

    public override List<SystemSet> GetDefaultSystemSets()
    {
        return new List<SystemSet>([new SystemTypeSet(this)]);
    }

    public override void UpdateTableComponentAccess(TableCache cache) { }
}
