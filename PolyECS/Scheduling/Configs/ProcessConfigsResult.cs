using Friflo.Json.Fliox.Mapper.Diff;
using PolyFlecs.Systems.Graph;

namespace PolyFlecs.Systems.Configs;

public struct ProcessConfigsResult
{
    /// <summary>
    /// All nodes contained inside this ProcessConfigs calls hierarchy, if ancestor_chained is true
    /// </summary>
    public List<NodeId> Nodes;
    /// <summary>
    /// True if and only if all nodes are "densely chained", meaning that all nested nodes are linearly chained (as if `after` system ordering has been applied
    /// between each node) in the order they are defined.
    /// </summary>
    public bool DenselyChained;

    public ProcessConfigsResult(List<NodeId> nodes, bool b)
    {
        Nodes = nodes;
        DenselyChained = b;
    }
}
