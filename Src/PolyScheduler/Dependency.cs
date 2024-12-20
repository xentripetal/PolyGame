namespace PolyScheduler;

/// <summary>
///     An edge to be added to the dependency graph.
///     Based on bevy_ecs::schedule::graph_utils::Dependency
/// </summary>
public struct Dependency {
    public enum Kind {
        /// <summary>A node that should be preceded.</summary>
        Before,

        /// <summary>A node that should be succeeded.</summary>
        After,

        /// <summary>
        ///     A node that should be preceded and will **not** automatically insert an instance of `apply_deferred` on the
        ///     edge.
        /// </summary>
        BeforeNoSync,

        /// <summary>
        ///     A node that should be succeeded and will **not** automatically insert an instance of `apply_deferred` on the
        ///     edge.
        /// </summary>
        AfterNoSync
    }

    public Kind DependencyKind;
    public ISystemSet Set;

    public Dependency(Kind kind, ISystemSet set) {
        DependencyKind = kind;
        Set = set;
    }

    public void Deconstruct(out Kind kind, out ISystemSet set) {
        kind = DependencyKind;
        set = Set;
    }
}