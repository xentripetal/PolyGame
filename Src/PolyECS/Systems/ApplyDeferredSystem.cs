namespace PolyECS.Systems;

/// <summary>
///     A special marker system that applies deferred buffers.
/// </summary>
public sealed class ApplyDeferredSystem : ClassSystem
{
    protected override void BuildParameters(ParamBuilder builder)
    { }

    public override void Initialize(PolyWorld world)
    {
        Meta.Access.CombinedAccess.WriteAll();
        Meta.StorageAccess.WriteAll();
        Meta.HasDeferred = false;
        Meta.IsExclusive = true;
    }

    public override List<ISystemSet> GetDefaultSystemSets() => [new SystemReferenceSet(this)];

    public override void Run(PolyWorld world)
    { }
}