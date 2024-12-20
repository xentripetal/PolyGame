using PolyECS.Scheduling;
using PolyECS.Scheduling.Configs;
using PolyECS.Systems;

namespace PolyECS;

/// <summary>
/// An AutoSystem is a system that uses PolyECS.Generator to generate its plumbing to be used in the ECS system.
/// </summary>
public abstract class AutoSystem : RunSystem
{
    protected AutoSystem()
    {
    }

    protected AutoSystem(string name) : base(name)
    {
    }

    public NodeConfigs<BaseSystem<Empty>> IntoConfigs()
    {
        IIntoNodeConfigs<BaseSystem<Empty>> baseConfig = NodeConfigs<BaseSystem<Empty>>.Of(new SystemConfig(this));

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

    // Re-export all the interface methods from IIntoSystemConfigs to make it easier to chain them

    public IIntoNodeConfigs<BaseSystem<Empty>> InSet(IIntoSystemSet set) => IntoConfigs().InSet(set);

    public IIntoNodeConfigs<BaseSystem<Empty>> InSet<TEnum>(TEnum set) where TEnum : struct, Enum =>
        IntoConfigs().InSet(set);

    public IIntoNodeConfigs<BaseSystem<Empty>> Before(IIntoSystemSet set) => IntoConfigs().Before(set);

    public IIntoNodeConfigs<BaseSystem<Empty>> After(IIntoSystemSet set) => IntoConfigs().After(set);

    public IIntoNodeConfigs<BaseSystem<Empty>> BeforeIgnoreDeferred(IIntoSystemSet set) =>
        IntoConfigs().BeforeIgnoreDeferred(set);

    public IIntoNodeConfigs<BaseSystem<Empty>> AfterIgnoreDeferred(IIntoSystemSet set) =>
        IntoConfigs().AfterIgnoreDeferred(set);

    public IIntoNodeConfigs<BaseSystem<Empty>> Chained() => IntoConfigs().Chained();

    public IIntoNodeConfigs<BaseSystem<Empty>> ChainedIgnoreDeferred() => IntoConfigs().ChainedIgnoreDeferred();

    public IIntoNodeConfigs<BaseSystem<Empty>> RunIf(Condition condition) => IntoConfigs().RunIf(condition);

    public IIntoNodeConfigs<BaseSystem<Empty>> DistributiveRunIf(Condition condition) =>
        IntoConfigs().DistributiveRunIf(condition);

    public IIntoNodeConfigs<BaseSystem<Empty>> AmbiguousWith(IIntoSystemSet set) => IntoConfigs().AmbiguousWith(set);

    public IIntoNodeConfigs<BaseSystem<Empty>> AmbiguousWithAll() => IntoConfigs().AmbiguousWithAll();

    public ISystemSet IntoSystemSet() => new SystemReferenceSet(this);
}