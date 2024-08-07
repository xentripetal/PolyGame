using Flecs.NET.Core;
using PolyECS.Systems;
using PolyECS.Systems.Executor;
using PolyECS.Systems.Graph;

namespace PolyECS.Scheduling.Executor;

public class SingleThreadedExecutor : IExecutor
{
    /// <summary>
    /// System sets whose conditions have been evaluated
    /// </summary>
    protected FixedBitSet EvaluatedSets;
    /// <summary>
    /// Systems that have run or been skipped
    /// </summary>
    protected FixedBitSet CompletedSystems;
    /// <summary>
    /// Applies deferred system buffers after all systems have ran
    /// </summary>
    protected bool ApplyFinalDeferred = true;

    public SingleThreadedExecutor() { }

    public void Init(SystemSchedule schedule)
    {
        int sysCount = schedule.SystemIds.Count;
        int setCount = schedule.SetIds.Count;
        EvaluatedSets = new FixedBitSet(setCount);
        CompletedSystems = new FixedBitSet(sysCount);
    }

    public void SetApplyFinalDeferred(bool apply)
    {
        ApplyFinalDeferred = apply;
    }

    protected void ApplyDeferred(SystemSchedule schedule, PolyWorld world)
    {
        world.DeferEnd();
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
            if (system is ApplyDeferredSystem)
            {
                ApplyDeferred(schedule, world);
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

        if (ApplyFinalDeferred)
        {
            ApplyDeferred(schedule, world);
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
            if (!condition.Evaluate(world))
            {
                met = false;
            }
        }
        return met;
    }
}
