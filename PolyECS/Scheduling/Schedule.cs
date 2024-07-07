using Flecs.NET.Core;
using PolyECS.Scheduling.Executor;
using PolyECS.Scheduling.Graph;
using PolyECS.Systems.Configs;
using PolyECS.Systems.Executor;
using PolyECS.Systems.Graph;
using QuikGraph;

namespace PolyECS.Systems;

/// <summary>
/// A collection of systems, and the metadata and executor needed to run them
/// in a certain order under certain conditions.
/// </summary>
public class Schedule
{
    public string Label { get; protected set; }
    internal SystemGraph Graph;
    internal SystemSchedule Executable;
    internal IExecutor Executor;
    protected bool ExecutorInitialized;

    public Schedule(string label = "default")
    {
        Label = label;
        Graph = new SystemGraph();
        Executable = new SystemSchedule();
        Executor = new SimpleExecutor();
    }

    public string GetLabel()
    {
        return Label;
    }

    /// <summary>
    /// Add a collection of systems to the schedule.
    /// </summary>
    /// <param name="configs"></param>
    /// <returns></returns>
    public Schedule AddSystems(NodeConfigs<RunSystem> configs)
    {
        Graph.ProcessConfigs(configs, false);
        return this;
    }

    public Schedule AddSystems(RunSystem[] systems, Condition[]? collectiveConditions = null, Chain chained = Chain.No)
    {
        return AddSystems(SystemConfigs.Of(systems, collectiveConditions: collectiveConditions, chained: chained));
    }

    public Schedule AddSystems(NodeConfigs<RunSystem>[] configs, Condition[]? collectiveConditions = null, Chain chained = Chain.No)
    {
        return AddSystems(SystemConfigs.Of(configs, collectiveConditions: collectiveConditions, chained: chained));
    }

    /// <summary>
    /// Suppress warnings and errors that would result from systems in these sets having ambiguities (Conflicting access but indeterminate order) with systems in set.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Schedule IgnoreAmbiguity(SystemSet a, SystemSet b)
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

    public ScheduleBuildSettings GetBuildSettings()
    {
        return Graph.Config;
    }

    public Schedule SetExecutor(IExecutor executor)
    {
        Executor = executor;
        return this;
    }

    public IExecutor GetExecutor()
    {
        return this.Executor;
    }

    /// <summary>
    /// Set whether the schedule applies deferred system buffers on final time or not. This is a catch-all
    /// in case a system uses commands but was not explicitly ordered before an instance of
    /// [`apply_deferred`]. By default this setting is true, but may be disabled if needed.
    /// </summary>
    /// <param name="apply"></param>
    /// <returns></returns>
    public Schedule SetApplyFinalDeferred(bool apply)
    {
        Executor.SetApplyFinalDeferred(apply);
        return this;
    }

    public void Run(World scheduleWorld)
    {
        Initialize(scheduleWorld);
        // TODO resource system to get skip systems
        Executor.Run(Executable, scheduleWorld, null);
    }

    public void Initialize(World scheduleWorld)
    {
        if (Graph.Changed)
        {
            Graph.Initialize(scheduleWorld);
            // TODO - resource system to get Schedules ambiguities
            Executable = Graph.UpdateSchedule(Executable, new HashSet<UntypedComponent>(), Label);
            Graph.Changed = false;
            ExecutorInitialized = false;
        }

        if (!ExecutorInitialized)
        {
            Executor.Init(Executable);
            ExecutorInitialized = true;
        }
    }
}
