using PolyECS.Systems;

namespace PolyECS.Scheduling.Configs;

public interface IIntoSystemSet
{
    public ISystemSet IntoSystemSet();
}

public interface IIntoNodeConfigs<T>
{
    public NodeConfigs<T> IntoConfigs();

    /// <summary>
    ///     Adds these systems to the provided <see cref="set" />
    /// </summary>
    /// <param name="set">Set to put systems in</param>
    /// <returns></returns>
    public NodeConfigs<T> InSet(IIntoSystemSet set) => IntoConfigs().InSet(set);

    public NodeConfigs<T> InSet<TEnum>(TEnum set) where TEnum : struct, Enum => IntoConfigs().InSet(set);

    public NodeConfigs<T> Before(IIntoSystemSet set) => IntoConfigs().Before(set);

    public NodeConfigs<T> After(IIntoSystemSet set) => IntoConfigs().After(set);

    public NodeConfigs<T> BeforeIgnoreDeferred(IIntoSystemSet set) => IntoConfigs().BeforeIgnoreDeferred(set);

    public NodeConfigs<T> AfterIgnoreDeferred(IIntoSystemSet set) => IntoConfigs().AfterIgnoreDeferred(set);

    public NodeConfigs<T> Chained() => IntoConfigs().Chained();

    public NodeConfigs<T> ChainedIgnoreDeferred() => IntoConfigs().ChainedIgnoreDeferred();

    public NodeConfigs<T> RunIf(ICondition condition) => IntoConfigs().RunIf(condition);

    public NodeConfigs<T> DistributiveRunIf(ICondition condition) => IntoConfigs().DistributiveRunIf(condition);

    public NodeConfigs<T> AmbiguousWith(IIntoSystemSet set) => IntoConfigs().AmbiguousWith(set);

    public NodeConfigs<T> AmbiguousWithAll() => IntoConfigs().AmbiguousWithAll();
}

public interface IIntoSystemConfigs : IIntoNodeConfigs<ISystem> { }
public interface IIntoSystemSetConfigs : IIntoNodeConfigs<ISystemSet> { }
