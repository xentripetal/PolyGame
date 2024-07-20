using PolyECS.Systems.Configs;

namespace PolyECS.Systems;

/// <summary>
/// A system that does not have input or output data. cannot use Void as its nots supported for generics. Instead, use object and just ignore them
/// </summary>
public abstract class RunSystem : BaseSystem<object?, object?>
{
    public static implicit operator SystemConfig(RunSystem sys)
    {
        return new SystemConfig(sys);
    }

    public static implicit operator NodeConfigs<RunSystem>(RunSystem sys)
    {
        return (SystemConfig)sys;
    }

    public static implicit operator SystemConfigs(RunSystem sys)
    {
        return (SystemConfigs)SystemConfigs.Of((SystemConfig)sys);
    }
}
