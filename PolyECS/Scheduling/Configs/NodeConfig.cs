using PolyECS.Scheduling.Graph;
using PolyECS.Systems.Graph;

namespace PolyECS.Systems.Configs;

/// <summary>
/// Stores configuration for a single generic node (a system or a system set)
///
/// The configuration includes the node itself, scheduling metadata
/// (hierarchy: in which sets is the node contained,
/// dependencies: before/after which other nodes should this node run)
/// and the run conditions associated with this node.
///
/// Port of bevy_ecs::schedule::config::NodeConfig
/// </summary>
public abstract class NodeConfig<T, TC>
{
    public T Node;
    /// <summary>
    /// Hierarchy and depdendency metadata for this node
    /// </summary>
    public SubgraphInfo Subgraph;
    public List<Condition> Conditions;

    public abstract NodeId ProcessConfig(SystemGraph<TC> graph);
}

public class SystemConfig<T> : NodeConfig<System<T>, T>
{
    public override NodeId ProcessConfig(SystemGraph<T> graph) => throw new NotImplementedException();
}

