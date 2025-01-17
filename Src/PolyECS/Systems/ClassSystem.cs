using Flecs.NET.Core;
using PolyECS.Scheduling.Configs;

namespace PolyECS.Systems;

/// <summary>
/// Base properties for a <see cref="ISystem"/> or <see cref="ICondition"/>.
/// </summary>
public interface IMetaSystem
{
    /// <summary>
    /// Gets the metadata and access information for a system
    /// </summary>
    public SystemMeta Meta { get; }

    /// <summary>
    /// Initialize the system and its parameters.
    /// </summary>
    /// <param name="world"></param>
    public void Initialize(PolyWorld world);

    /// <summary>
    /// Called whenever a new table is created. The system should check if the table has any components that it is interested in and update
    /// its metadata accordingly.
    /// </summary>
    /// <param name="cache"></param>
    public void UpdateStorageAccess(TableCache tables, ResourceStorage resources);
}

public interface ICondition : IMetaSystem
{
    /// <summary>
    /// Execute the condition and return the result
    /// </summary>
    /// <param name="world"></param>
    public bool Evaluate(PolyWorld world);
}

public interface ISystem : IMetaSystem
{
    /// <summary>
    /// Gets the system set that should be created alongside this system and it should be inserted into.
    /// </summary>
    /// <returns></returns>
    public List<ISystemSet> GetDefaultSystemSets();


    /// <summary>
    /// Execute the system
    /// </summary>
    /// <param name="world"></param>
    public void TryRun(PolyWorld world);
}

public abstract class ClassSystem : ISystem, IIntoSystemConfigs, IIntoSystemSet
{
    protected ClassSystem(params ISystemParam[] parameters)
    {
        Meta = new SystemMeta(GetType().Name);
        Params = parameters.ToList();
    }

    protected void SetupFromBuilder(ParamBuilder builder)
    {
        BuildParameters(builder);
        Params.AddRange(builder.Build());
    }

    protected abstract void BuildParameters(ParamBuilder builder);


    protected List<ISystemParam> Params;
    public SystemMeta Meta { get; }
    public virtual void Initialize(PolyWorld world)
    {
        SetupFromBuilder(world.GetParamBuilder());

        foreach (var param in Params)
        {
            param.Initialize(world, Meta);
        }
    }

    protected int TableGeneration;
    protected int ResourceGeneration;

    public void UpdateStorageAccess(TableCache tables, ResourceStorage resources)
    {
        (var oldGeneration, TableGeneration) = (TableGeneration, tables.Generation);
        for (var i = oldGeneration; i < TableGeneration; i++)
        {
            var storage = new Storage(i, -1, tables[i]);
            foreach (var parameter in Params)
            {
                parameter.EvaluateNewStorage(Meta, storage);
            }
        }

        (oldGeneration, ResourceGeneration) = (ResourceGeneration, resources.Generation);
        for (var i = oldGeneration; i < ResourceGeneration; i++)
        {
            var storage = new Storage(resources[i]!.Value);
            foreach (var parameter in Params)
            {
                parameter.EvaluateNewStorage(Meta, storage);
            }
        }
    }

    public virtual List<ISystemSet> GetDefaultSystemSets()
    {
        return [new SystemTypeSet(GetType())];
    }

    public virtual void TryRun(PolyWorld world)
    {
        foreach (var param in Params)
        {
            if (!param.IsReady(world, Meta))
            {
                return;
            }
        }
        Run(world);
    }

    public abstract void Run(PolyWorld world);
    public NodeConfigs<ISystem> IntoConfigs()
    {
        IIntoNodeConfigs<ISystem> baseConfig = NodeConfigs<ISystem>.Of(new SystemConfig(this));

        // Apply any attributes of this type onto its base config
        var attributes = Attribute.GetCustomAttributes(GetType(), true);
        foreach (var attr in attributes)
        {
            if (attr is SystemConfigAttribute configAttr)
            {
                baseConfig = configAttr.Apply(baseConfig);
            }
        }

        return baseConfig.IntoConfigs();
    }

    public ISystemSet IntoSystemSet()
    {
        return new SystemTypeSet(GetType());
    }


    // Re-export all the interface methods from IIntoSystemConfigs to make it easier to chain them

    public IIntoNodeConfigs<ISystem> InSet(IIntoSystemSet set) => IntoConfigs().InSet(set);

    public IIntoNodeConfigs<ISystem> InSet<TEnum>(TEnum set) where TEnum : struct, Enum => IntoConfigs().InSet(set);

    public IIntoNodeConfigs<ISystem> Before(IIntoSystemSet set) => IntoConfigs().Before(set);

    public IIntoNodeConfigs<ISystem> After(IIntoSystemSet set) => IntoConfigs().After(set);

    public IIntoNodeConfigs<ISystem> BeforeIgnoreDeferred(IIntoSystemSet set) => IntoConfigs().BeforeIgnoreDeferred(set);

    public IIntoNodeConfigs<ISystem> AfterIgnoreDeferred(IIntoSystemSet set) => IntoConfigs().AfterIgnoreDeferred(set);

    public IIntoNodeConfigs<ISystem> Chained() => IntoConfigs().Chained();

    public IIntoNodeConfigs<ISystem> ChainedIgnoreDeferred() => IntoConfigs().ChainedIgnoreDeferred();

    public IIntoNodeConfigs<ISystem> RunIf(ICondition condition) => IntoConfigs().RunIf(condition);

    public IIntoNodeConfigs<ISystem> DistributiveRunIf(ICondition condition) => IntoConfigs().DistributiveRunIf(condition);

    public IIntoNodeConfigs<ISystem> AmbiguousWith(IIntoSystemSet set) => IntoConfigs().AmbiguousWith(set);

    public IIntoNodeConfigs<ISystem> AmbiguousWithAll() => IntoConfigs().AmbiguousWithAll();
}