namespace PolyECS.Systems;

public abstract class ParameterSystem<TParam, TIn, TOut> : BaseSystem<TIn, TOut>, IMetaSystem
{
    private ISystemParam<TParam>? _parameter;

    private PolyWorld? _world;

    protected List<ISystemSet> DefaultSets = new ();
    protected SystemMeta Meta;
    protected int TableGeneration;

    protected ParameterSystem(string name)
    {
        Meta = new SystemMeta(name);
        _parameter = null;
    }

    protected ParameterSystem()
    {
        Meta = new SystemMeta(GetType().Name);
        _parameter = null;
    }

    public override bool HasDeferred => Meta.HasDeferred;

    public override bool IsExclusive => Meta.ComponentAccessSet.CombinedAccess.WritesAll;

    public SystemMeta GetMeta() => Meta;

    /// <summary>
    ///     Lets the system declare its parameter after constructor
    /// </summary>
    /// <param name="world"></param>
    /// <returns></returns>
    protected abstract ISystemParam<TParam> CreateParam(PolyWorld world);

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

        _parameter = CreateParam(world);
        _parameter.Initialize(world, Meta);
    }

    public override TOut Run(TIn i, PolyWorld world)
    {
        var p = _parameter!.Get(world, Meta);
        return Run(i, p);
    }

    public abstract TOut Run(TIn input, TParam param);

    public override Access<ulong> GetAccess() => Meta.ComponentAccessSet.CombinedAccess;

    public override Access<TableComponentId> GetTableAccess() => Meta.TableComponentAccess;

    public override List<ISystemSet> GetDefaultSystemSets() => DefaultSets;

    public override void UpdateTableComponentAccess(TableCache cache)
    {
        (var oldGeneration, TableGeneration) = (TableGeneration, cache.Generation);
        for (var i = oldGeneration; i < TableGeneration; i++)
        {
            _parameter.EvaluateNewTable(Meta, cache[i], i);
        }
    }
}
