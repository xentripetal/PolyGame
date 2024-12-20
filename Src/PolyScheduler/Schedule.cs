using QuikGraph;

namespace PolyScheduler;

/// <summary>
/// A schedule of <see cref="ScheduleEntry{TContext}"/> that contain ordering and dependency information for systems.
/// </summary>
/// <remarks> Based on bevy_ecs::schedule::Schedule </remarks>
public class Schedule<TContext, TResource> where TContext : IContext<TContext> {
    private readonly ScheduleGraph<TContext, TResource> _graph;
    private CompiledSchedule<TContext> _compiledSchedule = new();
    private IExecutor<TContext> _executor;

    public IExecutor<TContext> Executor {
        get => _executor;
        set {
            _executorInitialized = false;
            _executor = value;
        }
    }

    private bool _executorInitialized;
    private bool _dirty = true;

    public Schedule(ICompatabilityResolver<TContext, TResource> compatabilityResolver, IExecutor<TContext> executor) {
        _graph = new ScheduleGraph<TContext, TResource>(compatabilityResolver);
        _executor = executor;
    }

    public void Initialize(TContext context, ISet<TResource> ignoredAmbiguities) {
        if (_graph.Dirty) {
            _graph.Initialize(context);
            _compiledSchedule = _graph.UpdateSchedule(_compiledSchedule, ignoredAmbiguities);
            _executorInitialized = false;
        }

        if (!_executorInitialized) {
            _executor.Initialize(_compiledSchedule);
            _executorInitialized = true;
        }
    }

    /// <summary>
    /// Adds the provided system to the schedule.
    /// </summary>
    /// <param name="system">System entry to add.</param>
    /// <returns></returns>
    public Schedule<TContext, TResource> AddSystem(SystemEntry<TContext, TResource> system) {
        return AddEntries(system);
    }

    public Schedule<TContext, TResource> AddSystems(params IEnumerable<SystemEntry<TContext, TResource>> systems) {
        return AddEntries(systems);
    }

    public Schedule<TContext, TResource> AddEntries<TNode>(
        params IEnumerable<ScheduleEntry<TNode, TContext, TResource>> entries) {
        foreach (var entry in entries) {
            _graph.AddEntry(entry);
        }

        return this;
    }

    /// <summary>
    /// Adds the provided set definition to the schedule. 
    /// </summary>
    /// <param name="set"></param>
    /// <returns>The updated schedule</returns>
    public Schedule<TContext, TResource> AddSet(SetEntry<TContext, TResource> set) {
        return AddEntries(set);
    }

    public Schedule<TContext, TResource> AddSets(params IEnumerable<SetEntry<TContext, TResource>> sets) {
        return AddEntries(sets);
    }

    /// <summary>
    ///     Suppress warnings and errors that would result from systems in these sets having ambiguities (Conflicting access
    ///     but indeterminate order) with systems in set.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>The updated schedule</returns>
    public Schedule<TContext, TResource> IgnoreAmbiguity(ISystemSet a, ISystemSet b) {
        var hasA = _graph.SystemSetIds.TryGetValue(a, out var aNode);
        if (!hasA) {
            throw new ArgumentException(
                $"Could not mark system as ambiguous, {a} was not found in the schedule. Did you try to call IgnoreAmbiguity before adding the system to the schedule?");
        }

        var hasB = _graph.SystemSetIds.TryGetValue(b, out var bNode);
        if (!hasB) {
            throw new ArgumentException(
                $"Could not mark system as ambiguous, {b} was not found in the schedule. Did you try to call IgnoreAmbiguity before adding the system to the schedule?");
        }

        _graph.PermittedSetAmbiguities.AddEdge(new Edge<NodeId>(aNode, bNode));
        _dirty = true;
        return this;
    }
}