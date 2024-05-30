using PolyFlecs.Systems;
using PolyFlecs.Systems.Graph;

namespace PolyECS.Scheduling.Graph;

/// <summary>
/// Metadata about how the node fits in the schedule graph.
/// Based on bevy_ecs::schedule::graph_utils::GraphInfo
/// </summary>
public class SubgraphInfo
{
    /// <summary>
    /// The sets that the node belongs to (hierarchy)
    /// </summary>
    public List<SystemSet> Hierarchy = new ();
    /// <summary>
    /// The sets that the node depends on (must run before or after)
    /// </summary>
    public List<Dependency> Dependencies = new ();
    /// <summary>
    /// How to handle ambiguities with this node.
    /// </summary>
    public Ambiguity AmbiguousWith = new Ambiguity.Check();
    
    /// <summary>
    /// Marks the given set as ambiguous with this node. If the node is already marked as globally ambiguous, this does nothing.
    /// </summary>
    /// <param name="set"></param>
    public void AddAmbiguousWith(SystemSet set)
    {
        if (AmbiguousWith is Ambiguity.IgnoreWithSet ignoreWithSet)
        {
            ignoreWithSet.Sets.Add(set);
        }
        else if (AmbiguousWith is Ambiguity.Check)
        {
            AmbiguousWith = new Ambiguity.IgnoreWithSet([set]);
        }
    }
}

/// <summary>
/// An edge to be added to the dependency graph.
/// Based on bevy_ecs::schedule::graph_utils::Dependency
/// </summary>
public struct Dependency
{
    public DependencyKind Kind;
    public SystemSet Set;
    
    public Dependency(DependencyKind kind, SystemSet set)
    {
        Kind = kind;
        Set = set;
    }

    public void Deconstruct(out DependencyKind kind, out SystemSet set)
    {
        kind = Kind;
        set = Set;
    }
}

public abstract record Ambiguity
{
    /// <summary>
    /// Default ambiguity handling: check for conflicts
    /// </summary>
    public record Check : Ambiguity { }

    /// <summary>
    /// Ignore warnings with systems in any of these system sets. May contain duplicates.
    /// </summary>
    public record IgnoreWithSet(List<SystemSet> Sets) : Ambiguity;
    /// <summary>
    /// Ignore all warnings.
    /// </summary>
    public record IgnoreAll : Ambiguity { }
}
