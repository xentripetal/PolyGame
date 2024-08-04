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
    /// Adds these systems to the provided <see cref="set"/>
    /// </summary>
    /// <param name="set">Set to put systems in</param>
    /// <returns></returns>
    public NodeConfigs<T> InSet(IIntoSystemSet set)
    {
        return IntoConfigs().InSet(set);
    }
    
    public NodeConfigs<T> InSet<TEnum>(TEnum set) where TEnum : struct, Enum
    {
        return IntoConfigs().InSet(set);
    }
    
    public NodeConfigs<T> Before(IIntoSystemSet set)
    {
        return IntoConfigs().Before(set);
    }
    
    public NodeConfigs<T> After(IIntoSystemSet set)
    {
        return IntoConfigs().After(set);
    }
    
    public NodeConfigs<T> BeforeIgnoreDeferred(IIntoSystemSet set)
    {
        return IntoConfigs().BeforeIgnoreDeferred(set);
    }
    
    public NodeConfigs<T> AfterIgnoreDeferred(IIntoSystemSet set)
    {
        return IntoConfigs().AfterIgnoreDeferred(set);
    }

    public NodeConfigs<T> Chained()
    {
        return IntoConfigs().SetChained();
    }
    
    public NodeConfigs<T> ChainedIgnoreDeferred()
    {
        return IntoConfigs().SetChainedIgnoreDeferred();
    }
    
    public NodeConfigs<T> RunIf(Condition condition)
    {
        return IntoConfigs().RunIf(condition);
    }
    
    public NodeConfigs<T> DistributiveRunIf(Condition condition)
    {
        return IntoConfigs().DistributiveRunIf(condition);
    }
    
    public NodeConfigs<T> AmbiguousWith(IIntoSystemSet set)
    {
        return IntoConfigs().AmbiguousWith(set);
    }
    
    public NodeConfigs<T> AmbiguousWithAll()
    {
        return IntoConfigs().AmbiguousWithAll();
    }
    
}


public interface IIntoSystemConfigs : IIntoNodeConfigs<RunSystem>
{
    
}
