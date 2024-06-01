using PolyECS;
using PolyECS.Systems;
using PolyECS.Systems.Executor;
using PolyECS.Systems.Graph;

namespace PolyECS.Scheduling.Executor;

public class MultiThreadedExecutor<T> : IExecutor<T>
{
    public struct State
    {
        /// Metadata for scheduling and running system tasks.
        system_task_metadata: Vec<SystemTaskMetadata>,

        /// Union of the accesses of all currently running systems.
        active_access: Access<ArchetypeComponentId>,

        /// Returns `true` if a system with non-`Send` access is running.
        local_thread_running: bool,

        /// Returns `true` if an exclusive system is running.
        exclusive_running: bool,

        /// The number of systems that are running.
        num_running_systems: usize,

        /// The number of dependencies each system has that have not completed.
        num_dependencies_remaining: Vec<usize>,

        /// System sets whose conditions have been evaluated.
        evaluated_sets: FixedBitSet,

        /// Systems that have no remaining dependencies and are waiting to run.
        ready_systems: FixedBitSet,

        /// copy of `ready_systems`
        ready_systems_copy: FixedBitSet,

        /// Systems that are running.
        running_systems: FixedBitSet,

        /// Systems that got skipped.
        skipped_systems: FixedBitSet,

        /// Systems whose conditions have been evaluated and were run or skipped.
        completed_systems: FixedBitSet,

        /// Systems that have run but have not had their buffers applied.
        unapplied_systems: FixedBitSet,
    }



    /// <summary>
    /// System sets whose conditions have been evaluated
    /// </summary>
    protected FixedBitSet EvaluatedSets;
    /// <summary>
    /// Systems that have run or been skipped
    /// </summary>
    protected FixedBitSet CompletedSystems;
    /// <summary>
    /// Systems that have run but not had their buffers applied
    /// </summary>
    protected FixedBitSet UnappliedSystems;
    /// <summary>
    /// Applies deferred system buffers after all systems have ran
    /// </summary>
    protected bool ApplyFinalDeferred = true;

    public MultiThreadedExecutor() { }

    public void Init(SystemSchedule schedule)
    {
        int sysCount = schedule.SystemIds.Count;
        int setCount = schedule.SetIds.Count;
        EvaluatedSets = new FixedBitSet(setCount);
        CompletedSystems = new FixedBitSet(sysCount);
        UnappliedSystems = new FixedBitSet(sysCount);
    }

    public void SetApplyFinalDeferred(bool apply)
    {
        ApplyFinalDeferred = apply;
    }

    protected void ApplyDeferred(SystemSchedule schedule, IScheduleWorld world)
    {
        foreach (var systemIndex in UnappliedSystems.Ones())
        {
            var system = schedule.Systems[systemIndex];
            system.ApplyDeferred(world);
        }
        UnappliedSystems.Clear();
    }

    public void Run(SystemSchedule schedule, IScheduleWorld world, FixedBitSet? skipSystems)
    {
        if (skipSystems != null)
        {
            CompletedSystems.Or(skipSystems.Value);
        }

        for (int systemIndex = 0; systemIndex < schedule.Systems.Count; systemIndex++)
        {
            var shouldRun = !CompletedSystems.Contains(systemIndex);
            foreach (var setIdx in schedule.SetsWithConditionsOfSystems[systemIndex].Ones())
            {
                if (EvaluatedSets.Contains(setIdx))
                {
                    continue;
                }
                // Evaluate system set's conditions
                var setConditionsMet = EvaluateAndFoldConditions(schedule.SetConditions[setIdx], world);

                // Skip all systems that belong to this set, not just the current one
                if (!setConditionsMet)
                {
                    CompletedSystems.Or(schedule.SystemsInSetsWithConditions[setIdx]);
                }

                shouldRun &= setConditionsMet;
                EvaluatedSets.Set(setIdx);
            }

            // Evaluate System's conditions
            var systemConditionsMet = EvaluateAndFoldConditions(schedule.SystemConditions[systemIndex], world);
            shouldRun &= systemConditionsMet;

            CompletedSystems.Set(systemIndex);
            if (!shouldRun)
            {
                continue;
            }

            var system = schedule.Systems[systemIndex];
            if (system is ApplyDeferredSystem)
            {
                ApplyDeferred(schedule, world);
                continue;
            }

            try
            {
                if (system.IsExclusive)
                {
                    system.RunExclusive(world);
                }
                else
                {
                    system.RunDeferred(world);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in system {system.GetType().Name}: {e.Message}");
            }
            UnappliedSystems.Set(systemIndex);
        }

        if (ApplyFinalDeferred)
        {
            ApplyDeferred(schedule, world);
        }
        EvaluatedSets.Clear();
        CompletedSystems.Clear();
    }

    protected bool EvaluateAndFoldConditions(List<Condition> conditions, IScheduleWorld scheduleWorld)
    {
        // Not short-circuiting is intentional
        bool met = true;
        foreach (var condition in conditions)
        {
            if (!condition.Evaluate(scheduleWorld))
            {
                met = false;
            }
        }
        return met;
    }
}
