using PolyECS.Scheduling.Configs;

namespace PolyECS.Systems;

public abstract class BaseSystem<TOut>
{
    public BaseSystem()
    {
        var fullName = GetType().FullName;
        if (fullName != null) Meta = new SystemMeta(fullName);
        else throw new ApplicationException("The system class is missing or invalid.");
    }

    public BaseSystem(string name)
    {
        Meta = new SystemMeta(name);
    }

    protected SystemMeta Meta;
    protected List<ResParamMetadata> Params = new();

    public string Name => Meta.Name;

    /// <summary>
    ///     Returns `true` if the system performs any deferrable operations
    /// </summary>
    public virtual bool HasDeferred => Meta.HasDeferred;

    public virtual bool IsExclusive => Meta.ComponentAccessSet.CombinedAccess.WritesAll;
    public abstract void Initialize(PolyWorld world);

    public TOut RunWithChecks(PolyWorld world)
    {
        foreach (var parameter in Params)
        {
            if (!parameter.IsReady(world))
            {
                return default!;
            }
        }

        return Run(world);
    }

    protected abstract TOut Run(PolyWorld world);

    public virtual Access<ulong> GetComponentAccess() => Meta.ComponentAccessSet.CombinedAccess;
    public virtual Access<ulong> GetResourceAccess() => Meta.ResourceAccess;

    public virtual Access<TableComponentId> GetTableAccess() => Meta.TableComponentAccess;

    public abstract List<ISystemSet> GetDefaultSystemSets();

    protected int TableGeneration;
    protected int ResourceGeneration;

    public void UpdateAccess(TableCache cache, ResourceStorage resources)
    {
        (var oldGeneration, TableGeneration) = (TableGeneration, cache.Generation);
        for (var i = oldGeneration; i < TableGeneration; i++)
        {
            var storage = new Storage
            {
                Type = StorageType.Table,
                Generation = i,
                Table = cache[i]
            };
            foreach (var parameter in Params)
            {
                parameter.EvaluateNewStorage(Meta, storage);
            }
        }

        (var oldResourceGeneration, ResourceGeneration) = (ResourceGeneration, resources.Count);
        for (var i = oldResourceGeneration; i < ResourceGeneration; i++)
        {
            var storage = new Storage
            {
                Type = StorageType.Resource,
                Generation = i,
                Resource = resources[i]
            };
            foreach (var parameter in Params)
            {
                parameter.EvaluateNewStorage(Meta, storage);
            }
        }
    }
}

public abstract class Condition : BaseSystem<bool>, IIntoCondition
{
    protected Condition(string name) : base(name)
    {
    }

    public BaseSystem<bool> IntoCondition(PolyWorld world) => this;
}

public abstract class RunSystem : BaseSystem<Empty>, IIntoSystemConfigs, IIntoSystem
{
    protected RunSystem() : base()
    {
    }

    protected RunSystem(string name) : base(name)
    {
    }

    public NodeConfigs<BaseSystem<Empty>> IntoConfigs() =>
        new NodeConfigs<BaseSystem<Empty>>.Node(new SystemConfig(this));

    public BaseSystem<Empty> IntoSystem(PolyWorld world) => this;
}