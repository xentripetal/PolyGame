using PolyECS.Scheduling.Graph;
using PolyECS.Systems.Graph;

namespace PolyECS.Systems.Configs;

public class SystemSetConfig : NodeConfig<SystemSet>
{
    public SystemSetConfig(SystemSet set) : base(set) { }

    public override NodeId ProcessConfig(SystemGraph graph) => throw new NotImplementedException();
}
