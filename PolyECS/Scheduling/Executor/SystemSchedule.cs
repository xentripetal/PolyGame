namespace PolyECS.Systems.Graph;


/// <summary>
/// Holds systems and conditions of a [`Schedule`](super::Schedule) sorted in topological order
/// (along with dependency information for `multi_threaded` execution).
///
/// Since the arrays are sorted in the same order, elements are referenced by their index.
/// [`FixedBitSet`] is used as a smaller, more efficient substitute of `HashSet`.
/// </summary>
public class SystemSchedule
{
    /// <summary>
    /// Indexed by system node id
    /// </summary>
    public List<ASystem> Systems;

    /// <summary>
    /// List of system node ids.
    /// </summary>
    public List<NodeId> SystemIds;
    /// <summary>
    /// Indexed by system node id
    /// </summary>
    public List<List<Condition>> SystemConditions;
    /// <summary>
    /// Indexed by system node id
    /// Number of systems that immediately depend on the system
    /// </summary>
    public List<int> SystemDependencies;
    /// <summary>
    /// Indexed by system node id
    /// List of systems that immediately depend on the system
    /// </summary>
    public List<List<int>> SystemDependents;
    /// <summary>
    /// Indexed by system node id
    /// List of sets containing the system that have conditions
    /// </summary>
    public List<FixedBitSet> SetsWithConditionsOfSystems;
    /// <summary>
    /// List of system set node ids
    /// </summary>
    public List<NodeId> SetIds;
    /// <summary>
    /// Indexed by system set node id
    /// </summary>
    public List<List<Condition>> SetConditions;
    /// <summary>
    /// Indexed by system set node id.
    /// List of systems that are in sets that have conditions.
    ///
    /// If a set doesn't run because of its conditions, this is used to skip all systems in it.
    /// </summary>
    public List<FixedBitSet> SystemsInSetsWithConditions;
}
