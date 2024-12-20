namespace PolyScheduler;

/// <summary>
/// A compiled schedule that holds systems, conditions, and sets of a <see cref="Schedule{TContext}"/> sorted in
/// topological order (along with dependency information for `multi_threaded` execution).
/// Since the arrays are sorted in the same order, elements are referenced by their index.
/// <see cref="FixedBitSet"/> is used as a smaller, more efficient substitute of <see cref="HashSet{T}"/>
/// </summary>
public class CompiledSchedule<TContext>
{
    /// <summary>
    ///     Indexed by set node id
    /// </summary>
    public List<List<ICondition<TContext>>> SetConditions = new ();
    /// <summary>
    ///     List of set node ids
    /// </summary>
    public List<NodeId> SetIds = new ();
    /// <summary>
    ///     Indexed by system node id
    ///     List of sets containing the system that have conditions
    /// </summary>
    public List<FixedBitSet> SetsWithConditionsOfSystems = new ();
    /// <summary>
    ///     Indexed by system node id
    /// </summary>
    public List<List<ICondition<TContext>>> SystemConditions = new ();
    /// <summary>
    ///     Indexed by system node id
    ///     Number of systems that immediately depend on the system
    /// </summary>
    public List<int> SystemDependencies = new ();
    /// <summary>
    ///     Indexed by system node id
    ///     List of systems that immediately depend on the system
    /// </summary>
    public List<List<int>> SystemDependents = new ();

    /// <summary>
    ///     List of system node ids.
    /// </summary>
    public List<NodeId> SystemIds = new ();
    /// <summary>
    ///     Indexed by system node id
    /// </summary>
    public List<ISystem<TContext>> Systems = new ();
    /// <summary>
    ///     Indexed by system set node id.
    ///     List of systems that are in sets that have conditions.
    ///     If a set doesn't run because of its conditions, this is used to skip all systems in it.
    /// </summary>
    public List<FixedBitSet> SystemsInSetsWithConditions = new (); 
}