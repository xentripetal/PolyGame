using PolyECS.Scheduling.Executor;
using PolyECS.Scheduling.Graph;
using PolyECS.Systems.Configs;
using PolyECS.Systems.Executor;
using PolyECS.Systems.Graph;
using QuikGraph;
using TinyEcs;

namespace PolyECS.Systems;

/// <summary>
/// A collection of systems, and the metadata and executor needed to run them
/// in a certain order under certain conditions.
/// </summary>
public class Schedule
{
    protected string Label;
    protected SystemGraph Graph;
    protected SystemSchedule Executable;
    protected IExecutor Executor;
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
    public Schedule AddSystems(NodeConfigs<ASystem> configs)
    {
        Graph.ProcessConfigs(configs, false);
        return this;
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
            throw new ArgumentException($"Could not mark system as ambiguous, {a} was not found in the schedule. Did you try to call IgnoreAmbiguity before adding the system to the world?");
        }
        var hasB = Graph.SystemSetIds.TryGetValue(b, out var bNode);
        if (!hasB)
        {
            throw new ArgumentException($"Could not mark system as ambiguous, {b} was not found in the schedule. Did you try to call IgnoreAmbiguity before adding the system to the world?");
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
        //scheduleWorld.BeforeRun();
        // TODO resource system to get skip systems
        Executor.Run(Executable, scheduleWorld, null);
        //scheduleWorld.AfterRun();
    }

    public void Initialize(World scheduleWorld)
    {
        if (Graph.Changed)
        {
            Graph.Initialize(scheduleWorld);
            // TODO - resource system to get Schedules ambiguitites
            Graph.UpdateSchedule(Executable, new HashSet<ComponentInfo>(), Label);
            Graph.Changed = false;
            ExecutorInitialized = false;
        }

        if (!ExecutorInitialized)
        {
            Executor.Init(Executable);
            ExecutorInitialized = true;
        }
    }
    
    public void ApplyDeferred(World scheduleWorld)
    {
        foreach (var sys in Executable.Systems)
        {
            sys.ApplyDeferred(scheduleWorld);
        }
    }
}
