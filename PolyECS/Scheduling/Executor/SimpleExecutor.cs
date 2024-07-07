using Flecs.NET.Core;
using PolyECS;
using PolyECS.Systems;
using PolyECS.Systems.Executor;
using PolyECS.Systems.Graph;

namespace PolyECS.Scheduling.Executor;

public class SimpleExecutor : IExecutor
{
    /// <summary>
    /// System sets whose conditions have been evaluated
    /// </summary>
    protected FixedBitSet EvaluatedSets;
    /// <summary>
    /// Systems that have run or been skipped
    /// </summary>
    protected FixedBitSet CompletedSystems;

    public SimpleExecutor() { }

    public void Init(SystemSchedule schedule)
    {
        int sysCount = schedule.SystemIds.Count;
        int setCount = schedule.SetIds.Count;
        EvaluatedSets = new FixedBitSet(setCount);
        CompletedSystems = new FixedBitSet(sysCount);
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
            // Simple executor always applys deferred after a system, so skip inserted deferred systems
            if (system is ApplyDeferredSystem)
            {
                continue;
            }
            try
            {
                world.RunSystem(system);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in system {system.GetType().Name}: {e.Message}");
            }
        }
        EvaluatedSets.Clear();
        CompletedSystems.Clear();
    }

    protected bool EvaluateAndFoldConditions(List<Condition> conditions, PolyWorld world)
    {
        // Not short-circuiting is intentional
        bool met = true;
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
