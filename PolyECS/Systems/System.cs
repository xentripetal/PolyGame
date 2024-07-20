using Flecs.NET.Core;
using PolyECS.Scheduling.Graph;
using PolyECS.Systems.Configs;

namespace PolyECS.Systems;

public abstract class BaseSystem<TIn, TOut>
{
    public abstract void Initialize(PolyWorld world);
    public abstract TOut Run(TIn i, PolyWorld world);

    /// <summary>
    /// Returns `true` if the system performs any deferrable operations
    /// </summary>
    public virtual bool HasDeferred { get; }

    public virtual bool IsExclusive { get; }

    public abstract Access<ulong> GetAccess();

    public abstract Access<TableComponentId> GetTableAccess();

    public abstract List<SystemSet> GetDefaultSystemSets();

    public abstract void UpdateTableComponentAccess(TableCache cache);


}

