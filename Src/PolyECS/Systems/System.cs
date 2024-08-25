namespace PolyECS.Systems;

public abstract class BaseSystem<TIn, TOut>
{
    /// <summary>
    ///     Returns `true` if the system performs any deferrable operations
    /// </summary>
    public virtual bool HasDeferred { get; }

    public virtual bool IsExclusive { get; }
    public abstract void Initialize(PolyWorld world);
    public abstract TOut Run(TIn i, PolyWorld world);

    public abstract Access<ulong> GetAccess();

    public abstract Access<TableComponentId> GetTableAccess();

    public abstract List<ISystemSet> GetDefaultSystemSets();

    public abstract void UpdateTableComponentAccess(TableCache cache);
}
