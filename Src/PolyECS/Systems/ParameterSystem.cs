using Flecs.NET.Core;
using PolyECS.Scheduling.Graph;

namespace PolyECS.Systems;

public abstract class ParameterSystem<TParam, TIn, TOut> : BaseSystem<TIn, TOut>
{
    protected ParameterSystem(string name)
    {
        Meta = new SystemMeta(name);
        Parameter = null;
    }

    protected ParameterSystem()
    {
        Meta = new SystemMeta(GetType().Name);
        Parameter = null;
    }

    /// <summary>
    /// Lets the system declare its parameter after constructor
    /// </summary>
    /// <param name="world"></param>
    /// <returns></returns>
    protected abstract ISystemParam<TParam> CreateParam(PolyWorld world);

    protected SystemSet set;

    private PolyWorld? _world;
    protected SystemMeta Meta;
    private ISystemParam<TParam>? Parameter;
    protected int tableGeneration = 0;

    public override void Initialize(PolyWorld world)
    {
        if (_world != null)
        {
            if (world != _world)
            {
                throw new InvalidOperationException("System cannot be initialized with a different world than the one it was created with.");
            }
        }
        else
        {
            _world = world;
        }

        Parameter = CreateParam(world);
        Parameter.Initialize(world, Meta);
    }

    public override TOut Run(TIn i, PolyWorld world)
    {
        var p = Parameter.Get(world, Meta);
        return Run(i, p);
    }

    public abstract TOut Run(TIn input, TParam param);

    public override bool HasDeferred => Meta.HasDeferred;

    public override bool IsExclusive => Meta.ComponentAccessSet.CombinedAccess.WritesAll;

    public override Access<ulong> GetAccess()
    {
        return Meta.ComponentAccessSet.CombinedAccess;
    }

    public override Access<TableComponentId> GetTableAccess()
    {
        return Meta.TableComponentAccess;
    }

    protected List<SystemSet> DefaultSets = new List<SystemSet>();
    public override List<SystemSet> GetDefaultSystemSets() => DefaultSets;

    public override void UpdateTableComponentAccess(TableCache cache)
    {
        (var oldGeneration, tableGeneration) = (tableGeneration, cache.Generation);
        for (int i = oldGeneration; i < tableGeneration; i++)
        {
            Parameter.EvaluateNewTable(Meta, cache[i], i);
        }
    }
}
