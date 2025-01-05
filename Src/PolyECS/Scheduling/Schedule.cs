using PolyECS.Scheduling.Configs;
using PolyECS.Scheduling.Executor;
using PolyECS.Scheduling.Graph;
using PolyECS.Systems;
using QuikGraph;

namespace PolyECS.Scheduling;

/// <summary>
///     A collection of systems, and the metadata and executor needed to run them
///     in a certain order under certain conditions.
/// </summary>
public class Schedule
{
    internal SystemSchedule Executable;
    internal IExecutor Executor;
    protected bool ExecutorInitialized;
    internal readonly SystemGraph Graph;

    public Schedule(ScheduleLabel label)
    {
        Label = label;
        Graph = new SystemGraph();
        Executable = new SystemSchedule();
        Executor = new SimpleExecutor();
    }

    public ScheduleLabel Label { get; protected set; }

    public ScheduleLabel GetLabel() => Label;

    public Schedule AddSystems(params IIntoNodeConfigs<ISystem>[] configs)
    {
        foreach (var config in configs)
        {
            Graph.ProcessConfigs(config.IntoConfigs(), false);
        }
        return this;
    }

    /// <summary>
    ///     Suppress warnings and errors that would result from systems in these sets having ambiguities (Conflicting access
    ///     but indeterminate order) with systems in set.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Schedule IgnoreAmbiguity(ISystemSet a, ISystemSet b)
    {
        var hasA = Graph.SystemSetIds.TryGetValue(a, out var aNode);
        if (!hasA)
        {
            throw new ArgumentException(
                $"Could not mark system as ambiguous, {a} was not found in the schedule. Did you try to call IgnoreAmbiguity before adding the system to the world?");
        }
        var hasB = Graph.SystemSetIds.TryGetValue(b, out var bNode);
        if (!hasB)
        {
            throw new ArgumentException(
                $"Could not mark system as ambiguous, {b} was not found in the schedule. Did you try to call IgnoreAmbiguity before adding the system to the world?");
        }
        Graph.AmbiguousWith.AddEdge(new Edge<NodeId>(aNode, bNode));
        return this;
    }

    public Schedule SetBuildSettings(ScheduleBuildSettings settings)
    {
        Graph.Config = settings;
        return this;
    }

    public ScheduleBuildSettings GetBuildSettings() => Graph.Config;

    public Schedule SetExecutor(IExecutor executor)
    {
        Executor = executor;
        return this;
    }

    public IExecutor GetExecutor() => Executor;

    /// <summary>
    ///     Set whether the schedule applies deferred system buffers on final time or not. This is a catch-all
    ///     in case a system uses commands but was not explicitly ordered before an instance of
    ///     [`apply_deferred`]. By default, this setting is true, but may be disabled if needed.
    /// </summary>
    /// <param name="apply"></param>
    /// <returns></returns>
    public Schedule SetApplyFinalDeferred(bool apply)
    {
        Executor.SetApplyFinalDeferred(apply);
        return this;
    }

    public void Run(PolyWorld scheduleWorld)
    {
        Initialize(scheduleWorld);
        // TODO resource system to get skip systems
        Executor.Run(Executable, scheduleWorld, null);
    }

    public void Initialize(PolyWorld scheduleWorld)
    {
        if (Graph.Changed)
        {
            Graph.Initialize(scheduleWorld);
            // TODO - resource system to get Schedules ambiguities
            Executable = Graph.UpdateSchedule(scheduleWorld, Executable, new HashSet<AccessElement>(), Label);
            Graph.Changed = false;
            ExecutorInitialized = false;
        }

        if (!ExecutorInitialized)
        {
            Executor.Init(Executable);
            ExecutorInitialized = true;
        }
    }

    public Schedule ConfigureSets(IIntoNodeConfigs<ISystemSet> sets)
    {
        Graph.ConfigureSets(sets);
        return this;
    }

    public Schedule ConfigureSets(IIntoNodeConfigs<ISystemSet>[] sets)
    {
        foreach (var set in sets)
        {
            Graph.ConfigureSets(set);
        }
        return this;
    }
}
