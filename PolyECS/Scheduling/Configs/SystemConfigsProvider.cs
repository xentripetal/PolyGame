namespace PolyECS.Systems.Configs;

/// <summary>
/// Types that can convert into a <see cref="SystemConfig"/>
///
/// This trait is implemented for "systems" (functions whose arguments all implement
/// [`SystemParam`](crate::system::SystemParam)), or tuples thereof.
/// It is a common entry point for system configurations. 
/// </summary>
public abstract class SystemConfigsProvider
{
    public abstract NodeConfigs<System> IntoConfigs();

    public NodeConfigs<System> InSet(SystemSet set)
    {
        return IntoConfigs().InSet(set);
    }

    public NodeConfigs<System> Before(SystemSet set)
    {
        return IntoConfigs().Before(set);
    }

    public NodeConfigs<System> After(SystemSet set)
    {
        return IntoConfigs().After(set);
    }

    public NodeConfigs<System> BeforeIgnoreDeferred(SystemSet set)
    {
        return IntoConfigs().BeforeIgnoreDeferred(set);
    }

    public NodeConfigs<System> AfterIgnoreDeferred(SystemSet set)
    {
        return IntoConfigs().AfterIgnoreDeferred(set);
    }

    public NodeConfigs<System> DistributiveRunIf(Condition condition)
    {
        return IntoConfigs().DistributiveRunIf(condition);
    }

    public NodeConfigs<System> RunIf(Condition condition)
    {
        return IntoConfigs().RunIf(condition);
    }

    public NodeConfigs<System> AmbiguousWith(SystemSet set)
    {
        return IntoConfigs().AmbiguousWith(set);
    }

    public NodeConfigs<System> AmbiguousWithAll()
    {
        return IntoConfigs().AmbiguousWithAll();
    }
    
    public NodeConfigs<System> SetChained()
    {
        return IntoConfigs().SetChained();
    }
    
    public NodeConfigs<System> SetChainedIgnoreDeferred()
    {
        return IntoConfigs().SetChainedIgnoreDeferred();
    }
}
