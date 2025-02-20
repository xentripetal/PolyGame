using PolyECS.Systems;

namespace PolyECS.Scheduling.Executor;

public class SingleThreadedExecutor : IExecutor
{
    /// <summary>
    ///     Applies deferred system buffers after all systems have ran
    /// </summary>
    protected bool ApplyFinalDeferred = true;
    /// <summary>
    ///     Systems that have run or been skipped
    /// </summary>
    protected FixedBitSet CompletedSystems;
    /// <summary>
    ///     System sets whose conditions have been evaluated
    /// </summary>
    protected FixedBitSet EvaluatedSets;

    public void Init(SystemSchedule schedule)
    {
        var sysCount = schedule.SystemIds.Count;
        var setCount = schedule.SetIds.Count;
        EvaluatedSets = new FixedBitSet(setCount);
        CompletedSystems = new FixedBitSet(sysCount);
    }

    public void SetApplyFinalDeferred(bool apply)
    {
        ApplyFinalDeferred = apply;
    }

    public void Run(SystemSchedule schedule, PolyWorld world, FixedBitSet? skipSystems)
    {
        if (skipSystems != null)
        {
            CompletedSystems.Or(skipSystems.Value);
        }

        for (var systemIndex = 0; systemIndex < schedule.Systems.Count; systemIndex++)
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
                using var _ = Profiler.BeginZone(system.Meta.Name);
                world.RunSystem(system);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in system {system.GetType().Name}: {e.Message}");
            }
        }

        if (ApplyFinalDeferred)
        {
            ApplyDeferred(schedule, world);
        }
        EvaluatedSets.Clear();
        CompletedSystems.Clear();
    }

    protected void ApplyDeferred(SystemSchedule schedule, PolyWorld world)
    {
        world.DeferEnd();
    }

    protected bool EvaluateAndFoldConditions(List<ICondition> conditions, PolyWorld world)
    {
        // Not short-circuiting is intentional
        var met = true;
        foreach (var condition in conditions)
        {
            if (!condition.Evaluate(world))
            {
                met = false;
            }
        }
        return met;
    }
}
