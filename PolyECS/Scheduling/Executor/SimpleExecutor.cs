using PolyFlecs;
using PolyFlecs.Systems;
using PolyFlecs.Systems.Executor;
using PolyFlecs.Systems.Graph;

namespace PolyECS.Scheduling.Executor;

public class SimpleExecutor : IExecutor
{
    /// Systems sets whose conditions have been evaluated.
    protected FixedBitSet EvaluatedSets;
    protected FixedBitSet CompletedSystems;

    public SimpleExecutor() { }

    public void Init(SystemSchedule schedule)
    {
        int sysCount = schedule.SystemIds.Count;
        int setCount = schedule.SetIds.Count;
        EvaluatedSets = new FixedBitSet(sysCount);
        CompletedSystems = new FixedBitSet(setCount);
    }

    public void SetApplyFinalDeferred(bool apply)
    {
        // do nothing. simple executor does not do a final sync
    }

    public void Run(SystemSchedule schedule, World world, FixedBitSet? skipSystems)
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
            if (system.HasDeferred())
            {
                system.ApplyDeferred(world);
            }
            else
            {
                system.Update();
            }
        }
    }

    protected bool EvaluateAndFoldConditions(List<Condition> conditions, World world)
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
