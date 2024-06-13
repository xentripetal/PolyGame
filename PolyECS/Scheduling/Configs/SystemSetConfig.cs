using PolyECS.Scheduling.Graph;
using PolyECS.Systems.Graph;

namespace PolyECS.Systems.Configs;

public class SystemSetConfig<TC> : NodeConfig<SystemSet, TC>
{
    public SystemSet Set;
    public SystemSetConfig(SystemSet set)
    {
        Set = set;
    }
    
    public override NodeId ProcessConfig(SystemGraph<TC> graph) => throw new NotImplementedException();
}
