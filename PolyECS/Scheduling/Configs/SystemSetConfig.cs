using PolyECS.Scheduling.Graph;
using PolyFlecs.Systems.Graph;

namespace PolyFlecs.Systems.Configs;

public class SystemSetConfig : NodeConfig<SystemSet>
{
    public SystemSet Set;
    public SystemSetConfig(SystemSet set)
    {
        Set = set;
    }
    
    public override NodeId ProcessConfig(SystemGraph graph) => throw new NotImplementedException();
}
