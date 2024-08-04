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
    
    public NodeConfigs<T> InSet<T>(T set) where T : struct, Enum
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


public interface IIntoSystemConfigs
{
    public SystemConfigs IntoSystemConfig();

    /// <summary>
    /// Adds these systems to the provided <see cref="set"/>
    /// </summary>
    /// <param name="set">Set to put systems in</param>
    /// <returns></returns>
    public SystemConfigs InSet(IIntoSystemSet set)
    {
        return (SystemConfigs)IntoSystemConfig().InSet(set);
    }
    
    public SystemConfigs InSet<T>(T set) where T : struct, Enum
    {
        return (SystemConfigs)IntoSystemConfig().InSet(set);
    }
    
    public SystemConfigs Before(IIntoSystemSet set)
    {
        return (SystemConfigs)IntoSystemConfig().Before(set);
    }
    
    public SystemConfigs After(IIntoSystemSet set)
    {
        return (SystemConfigs)IntoSystemConfig().After(set);
    }
    
    public SystemConfigs BeforeIgnoreDeferred(IIntoSystemSet set)
    {
        return (SystemConfigs)IntoSystemConfig().BeforeIgnoreDeferred(set);
    }
    
    public SystemConfigs AfterIgnoreDeferred(IIntoSystemSet set)
    {
        return (SystemConfigs)IntoSystemConfig().AfterIgnoreDeferred(set);
    }

    public SystemConfigs Chained()
    {
        return (SystemConfigs)IntoSystemConfig().SetChained();
    }
    
    public SystemConfigs ChainedIgnoreDeferred()
    {
        return (SystemConfigs)IntoSystemConfig().SetChainedIgnoreDeferred();
    }
    
    public SystemConfigs RunIf(Condition condition)
    {
        return (SystemConfigs)IntoSystemConfig().RunIf(condition);
    }
    
    public SystemConfigs DistributiveRunIf(Condition condition)
    {
        return (SystemConfigs)IntoSystemConfig().DistributiveRunIf(condition);
    }
    
    public SystemConfigs AmbiguousWith(IIntoSystemSet set)
    {
        return (SystemConfigs)IntoSystemConfig().AmbiguousWith(set);
    }
    
    public SystemConfigs AmbiguousWithAll()
    {
        return (SystemConfigs)IntoSystemConfig().AmbiguousWithAll();
    }
    
}
