namespace PolyECS.Systems;

/// <summary>
///     A special marker system that applies deferred buffers.
/// </summary>
public sealed class ApplyDeferredSystem : RunSystem
{
    private readonly Access<ulong> access = new Access<ulong>().WriteAll();
    private readonly Access<TableComponentId> tableAccess = new Access<TableComponentId>().WriteAll();

    public override bool HasDeferred => false;
    public override bool IsExclusive => true;


    public override void Initialize(PolyWorld world) { }

    public override Empty Run(Empty _, PolyWorld world) => Empty.Instance;

    public override Access<ulong> GetAccess() => access;

    public override Access<TableComponentId> GetTableAccess() => tableAccess;

    public override List<ISystemSet> GetDefaultSystemSets() => new ([new SystemReferenceSet(this)]);

    public override void UpdateTableComponentAccess(TableCache cache) { }
}
