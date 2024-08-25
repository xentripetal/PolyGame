using PolyECS.Scheduling.Configs;

namespace PolyECS.Systems;

/// <summary>
///     A <see cref="RunSystem" /> that takes in a parameter
/// </summary>
/// <typeparam name="T">Parameter type</typeparam>
public abstract class ClassSystem<T> : RunnableSystem<T>
{
    protected ClassSystem(string name) : base(name)
    {
        DefaultSets.Add(new SystemTypeSet(GetType()));
    }

    protected ClassSystem()
    {
        DefaultSets.Add(new SystemTypeSet(GetType()));
    }


    public override Empty Run(Empty i, T param)
    {
        Run(param);
        return i;
    }

    public abstract void Run(T param);

    public static implicit operator NodeConfig<RunSystem>(ClassSystem<T> system) => new SystemConfig(system);

    public static implicit operator NodeConfigs<RunSystem>(ClassSystem<T> system) => new SystemConfig(system);
}
