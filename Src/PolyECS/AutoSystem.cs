using PolyECS.Scheduling;
using PolyECS.Scheduling.Configs;
using PolyECS.Systems;

namespace PolyECS;

/// <summary>
/// An AutoSystem is a system that uses PolyECS.Generator to generate its plumbing to be used in the ECS system.
/// </summary>
public abstract class AutoSystem : ParameterSystem<Empty, Empty>, IIntoSystemConfigs
{
    public override void Initialize(PolyWorld world)
    {
        Params = GetParams(world);
        base.Initialize(world);
    }

    public abstract ISystemParam[] GetParams(PolyWorld world);
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
