using PolyECS.Scheduling.Graph;
using PolyECS.Systems;
using PolyECS.Systems.Graph;

namespace PolyECS.Scheduling.Configs;

public class SystemSetConfig : NodeConfig<ISystemSet>
{
    public SystemSetConfig(ISystemSet set) : base(set) { }

    public override NodeId ProcessConfig(SystemGraph graph) => throw new NotImplementedException();
}
