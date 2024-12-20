namespace PolyECS.Systems;

/// <summary>
///     A special marker system that applies deferred buffers.
/// </summary>
public sealed class ApplyDeferredSystem : BaseSystem<Empty>
{
    public ApplyDeferredSystem() : base("ApplyDeferred")
    {
        GetTableAccess().WriteAll();
        GetResourceAccess().WriteAll();
    }

    public override bool HasDeferred => false;
    public override bool IsExclusive => true;
    public override void Initialize(PolyWorld world) { }

    protected override Empty Run(PolyWorld world)
    {
        world.World.DeferEnd();
        world.World.DeferBegin();
        return Empty.Instance;
    }

    public override List<ISystemSet> GetDefaultSystemSets() => [new SystemTypeSet<ApplyDeferredSystem>()];
}
