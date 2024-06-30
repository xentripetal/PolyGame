using Flecs.NET.Core;
using PolyECS.Scheduling.Graph;
using PolyECS.Systems.Configs;

namespace PolyECS.Systems;

public abstract class ASystem
{
    public abstract void Initialize(World world);
    public abstract void Run(World world);

    /// <summary>
    /// Returns `true` if the system has deferred buffers
    /// </summary>
    public bool HasDeferred { get; protected set; } = true;

    public bool IsExclusive { get; protected set; } = false;

    public abstract Access<UntypedComponent> GetAccess();

    public abstract List<SystemSet> GetDefaultSystemSets();

    public virtual SystemSet ToSystemSet()
    {
        return new SystemTypeSet(this);
    }

    public abstract void ApplyDeferred(World scheduleWorld);

    public static implicit operator SystemConfig(ASystem sys)
    {
        return new SystemConfig(sys);
    }

    public static implicit operator NodeConfigs<ASystem>(ASystem sys)
    {
        return (SystemConfig)sys;
    }
    
    public static implicit operator SystemConfigs(ASystem sys)
    {
        return (SystemConfigs)SystemConfigs.Of((SystemConfig)sys);
    }
}
