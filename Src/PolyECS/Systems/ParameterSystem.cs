using Flecs.NET.Core;

namespace PolyECS.Systems;

public abstract class TParameterSystem<TParam, TIn, TOut> : ParameterSystem<TIn, TOut>
{
    protected TParameterSystem(string name) : base(name) { }
    protected TParameterSystem() { }
    private ITSystemParam<TParam>? _parameter;

    /// <summary>
    ///     Lets the system declare its parameter after constructor
    /// </summary>
    /// <param name="world"></param>
    /// <returns></returns>
    protected abstract ITSystemParam<TParam> CreateParam(PolyWorld world);

    public override void Initialize(PolyWorld world)
    {
        _parameter = CreateParam(world);
        Params = new ISystemParam[]
        {
            _parameter
        };
        base.Initialize(world);
    }


    public override TOut Run(TIn i, PolyWorld world)
    {
        var p = _parameter!.Get(world, Meta);
        return Run(i, p);
    }

    public abstract TOut Run(TIn input, TParam param);
}

public abstract class ParameterSystem<TIn, TOut> : BaseSystem<TIn, TOut>, IMetaSystem
{
    protected ISystemParam[] Params;
    private PolyWorld? _world;

    protected List<ISystemSet> DefaultSets = new ();
    protected SystemMeta Meta;
    protected int TableGeneration;

    protected ParameterSystem(string name)
    {
        Meta = new SystemMeta(name);
        Params = new ISystemParam[0];
    }

    protected ParameterSystem()
    {
        Meta = new SystemMeta(GetType().Name);
    }

    public override bool HasDeferred => Meta.HasDeferred;

    public override bool IsExclusive => Meta.ComponentAccessSet.CombinedAccess.WritesAll;

    public SystemMeta GetMeta() => Meta;

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

        foreach (var param in Params)
        {
            param.Initialize(world, Meta);
        }
    }

    public override Access<ulong> GetAccess() => Meta.ComponentAccessSet.CombinedAccess;

    public override Access<TableComponentId> GetTableAccess() => Meta.TableComponentAccess;

    public override List<ISystemSet> GetDefaultSystemSets() => DefaultSets;

    public override void UpdateTableComponentAccess(TableCache cache)
    {
        (var oldGeneration, TableGeneration) = (TableGeneration, cache.Generation);
        for (var i = oldGeneration; i < TableGeneration; i++)
        {
            foreach (var parameter in Params)
            {
                parameter.EvaluateNewTable(Meta, cache[i], i);
            }
        }
    }
}
