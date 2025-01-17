using QuikGraph;

namespace PolyECS.Scheduling.Graph;

public class CheckGraphResults<V>
{
    /// Pairs of nodes that have a path connecting them.
    public HashSet<(V, V)> Connected = new();
    /// Pairs of nodes that don't have a path connecting them.
    public List<(V, V)> Disconnected = new();
    public FixedBitSet Reachable = new();
    /// Variant of the graph with all possible transitive edges.
    // TODO: this will very likely be used by "if-needed" ordering
    public BidirectionalGraph<V, Edge<V>> TransitiveClosure = new();
    /// Edges that are redundant because a longer path exists.
    public List<(V, V)> TransitiveEdges = new();
    /// Variant of the graph with no transitive edges.
    public BidirectionalGraph<V, Edge<V>> TransitiveReduction = new();
}
