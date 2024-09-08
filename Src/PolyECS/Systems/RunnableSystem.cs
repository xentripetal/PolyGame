using PolyECS.Scheduling;
using PolyECS.Scheduling.Configs;

namespace PolyECS.Systems;

public abstract class RunnableSystem : RunnableSystem<Empty>
{
    protected RunnableSystem(string name) : base(name) { }

    protected RunnableSystem() { }
}

/// <summary>
///     A parameter based system that takes no input and returns no output. Runnable denotes that it is the standard system
///     type that is ran by the scheduler <see cref="RunSystem" />
/// </summary>
/// <typeparam name="T">Parameter type</typeparam>
public abstract class RunnableSystem<T> : ParameterSystem<T, Empty, Empty>, IIntoSystemConfigs, IIntoSystemSet
{
    protected RunnableSystem(string name) : base(name) { }

    protected RunnableSystem() { }

    public NodeConfigs<RunSystem> IntoConfigs()
    {
        IIntoNodeConfigs<RunSystem> baseConfig = NodeConfigs<RunSystem>.Of(new SystemConfig(this));

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

    public IIntoNodeConfigs<RunSystem> InSet(IIntoSystemSet set) => IntoConfigs().InSet(set);

    public IIntoNodeConfigs<RunSystem> InSet<TEnum>(TEnum set) where TEnum : struct, Enum => IntoConfigs().InSet(set);

    public IIntoNodeConfigs<RunSystem> Before(IIntoSystemSet set) => IntoConfigs().Before(set);

    public IIntoNodeConfigs<RunSystem> After(IIntoSystemSet set) => IntoConfigs().After(set);

    public IIntoNodeConfigs<RunSystem> BeforeIgnoreDeferred(IIntoSystemSet set) => IntoConfigs().BeforeIgnoreDeferred(set);

    public IIntoNodeConfigs<RunSystem> AfterIgnoreDeferred(IIntoSystemSet set) => IntoConfigs().AfterIgnoreDeferred(set);

    public IIntoNodeConfigs<RunSystem> Chained() => IntoConfigs().Chained();

    public IIntoNodeConfigs<RunSystem> ChainedIgnoreDeferred() => IntoConfigs().ChainedIgnoreDeferred();

    public IIntoNodeConfigs<RunSystem> RunIf(Condition condition) => IntoConfigs().RunIf(condition);

    public IIntoNodeConfigs<RunSystem> DistributiveRunIf(Condition condition) => IntoConfigs().DistributiveRunIf(condition);

    public IIntoNodeConfigs<RunSystem> AmbiguousWith(IIntoSystemSet set) => IntoConfigs().AmbiguousWith(set);

    public IIntoNodeConfigs<RunSystem> AmbiguousWithAll() => IntoConfigs().AmbiguousWithAll();

    public ISystemSet IntoSystemSet() => new SystemReferenceSet(this);
}
