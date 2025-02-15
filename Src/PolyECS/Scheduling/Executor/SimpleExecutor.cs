using PolyECS.Systems;
using Serilog;

namespace PolyECS.Scheduling.Executor;

public class SimpleExecutor : IExecutor
{
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
        EvaluatedSets = new FixedBitSet(schedule.SetIds.Count);
        CompletedSystems = new FixedBitSet(schedule.SystemIds.Count);
    }

    public void SetApplyFinalDeferred(bool apply)
    {
        // do nothing. simple executor does not do a final sync
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
            // Simple executor always applys deferred after a system, so skip inserted deferred systems
            if (system is ApplyDeferredSystem)
            {
                continue;
            }
            try
            {
                using var _ = Profiler.BeginZone(system.Meta.Name);
                world.RunSystem(system);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in system {System}", system.GetType().Name);
            }
        }
        EvaluatedSets.Clear();
        CompletedSystems.Clear();
    }

    protected bool EvaluateAndFoldConditions(List<ICondition> conditions, PolyWorld world)
    {
        // Not short-circuiting is intentional
        var met = true;
        foreach (var condition in conditions)
        {
            // TODO refactor conditions
            if (!condition.Evaluate(world))
            {
                met = false;
            }
        }
        return met;
    }
}
