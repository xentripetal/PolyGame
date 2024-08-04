using PolyECS.Scheduling.Configs;

namespace PolyECS.Systems;

/// <summary>
/// A parameter based system that takes no input and returns no output. Runnable denotes that it is the standard system type that is ran by the scheduler <see cref="RunSystem"/>
/// </summary>
/// <typeparam name="T">Parameter type</typeparam>
public abstract class RunnableSystem<T> : ParameterSystem<T, Empty, Empty>, IIntoSystemConfigs, IIntoSystemSet
{
    protected RunnableSystem(string name) : base(name) { }

    protected RunnableSystem() : base() { }

    public SystemConfigs IntoSystemConfig()
    {
      IIntoSystemConfigs baseConfig = SystemConfigs.Of(new SystemConfig(this));  
      
      // Apply any attributes of this type onto its base config
      var attributes = Attribute.GetCustomAttributes(GetType(), true);
      foreach (var attr in attributes)
      {
          if (attr is SystemConfigAttribute configAttr)
          {
              baseConfig = configAttr.Apply(baseConfig);
          }
      }

      return baseConfig.IntoSystemConfig();
    } 

    public ISystemSet IntoSystemSet() => new SystemReferenceSet(this);

    // Re-export all the interface methods from IIntoSystemConfigs to make it easier to chain them
    public SystemConfigs InSet(IIntoSystemSet set)
    {
        return (SystemConfigs)IntoSystemConfig().InSet(set);
    }

    public SystemConfigs InSet<TEnum>(TEnum set) where TEnum : struct, Enum
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
