using PolyECS.Scheduling.Graph;
using PolyECS.Systems;

namespace PolyECS.Scheduling.Configs;

/// <summary>
///     Stores configuration for a single generic node (a system or a system set)
///     The configuration includes the node itself, scheduling metadata
///     (hierarchy: in which sets is the node contained,
///     dependencies: before/after which other nodes should this node run)
///     and the run conditions associated with this node.
///     Port of bevy_ecs::schedule::config::NodeConfig
/// </summary>
public abstract class NodeConfig<T> : IIntoNodeConfigs<T>
{
    public List<Condition> Conditions = new ();
    public T Node;
    /// <summary>
    ///     Hierarchy and depdendency metadata for this node
    /// </summary>
    public SubgraphInfo Subgraph = new ();

    protected NodeConfig(T node) => Node = node;

    public NodeConfigs<T> IntoConfigs() => NodeConfigs<T>.Of(this);

    public abstract NodeId ProcessConfig(SystemGraph graph);

    public static implicit operator NodeConfigs<T>(NodeConfig<T> config) => new NodeConfigs<T>.Node(config);
}

public class SystemConfig : NodeConfig<RunSystem>, IIntoSystemConfigs
{
    public SystemConfig(RunSystem system) : base(system)
    {
        Subgraph.Hierarchy = system.GetDefaultSystemSets();
        Conditions = new List<Condition>();
    }

    public override NodeId ProcessConfig(SystemGraph graph) => graph.AddSystem(this);
}

public class SystemSetConfig : NodeConfig<ISystemSet>, IIntoSystemSet, IIntoSystemSetConfigs
{
    public SystemSetConfig(ISystemSet set) : base(set) { }
    public ISystemSet IntoSystemSet() => Node;
    public NodeConfigs<ISystemSet> IntoConfigs() => this;

    public override NodeId ProcessConfig(SystemGraph graph) => graph.ConfigureSet(this);
}
