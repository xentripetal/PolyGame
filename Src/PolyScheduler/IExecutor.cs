namespace PolyScheduler;

public interface IExecutor<TContext> {
    /// <summary>
    /// Initialize the executor with the schedule to run. Will be called before any Run calls if the schedule
    /// has changed since last Run. 
    /// </summary>
    /// <param name="schedule"></param>
    void Initialize(CompiledSchedule<TContext> schedule);
    /// <summary>
    /// Whether the executor should apply any deferred operations from the systems it ran before finishing its run.
    /// Up to the executor implementation to respect this property.
    /// </summary>
    public bool ApplyFinalDeferred { get; set; }
    /// <summary>
    /// Run the provided executable using the provided context. The skipSystems parameter is a bitset of systems
    /// that should be skipped in the schedule. This is used to skip systems that have already been run in a previous
    /// </summary>
    /// <param name="executable"></param>
    /// <param name="context"></param>
    /// <param name="skipSystems"></param>
    void Run(CompiledSchedule<TContext> executable, TContext context, FixedBitSet? skipSystems);
}