namespace PolyScheduler;

public class SimpleExecutor<TContext> : IExecutor<TContext> where TContext : IContext<TContext> {
    /// <summary>
    ///     Systems that have run or been skipped
    /// </summary>
    protected FixedBitSet CompletedSystems;

    /// <summary>
    ///     System sets whose conditions have been evaluated
    /// </summary>
    protected FixedBitSet EvaluatedSets;

    public void Initialize(CompiledSchedule<TContext> schedule) {
        EvaluatedSets = new FixedBitSet(schedule.SetIds.Count);
        CompletedSystems = new FixedBitSet(schedule.SystemIds.Count);
    }

    public bool ApplyFinalDeferred { get; set; }

    public void Run(CompiledSchedule<TContext> executable, TContext context, FixedBitSet? skipSystems) {
        EvaluatedSets.Clear();
        CompletedSystems.Clear();
        
        if (skipSystems != null) {
            CompletedSystems.Or(skipSystems.Value);
        }

        for (var systemIndex = 0; systemIndex < executable.Systems.Count; systemIndex++) {
            var shouldRun = !CompletedSystems.Contains(systemIndex);
            foreach (var setIdx in executable.SetsWithConditionsOfSystems[systemIndex].Ones()) {
                if (EvaluatedSets.Contains(setIdx)) {
                    continue;
                }

                // Evaluate system set's conditions
                var setConditionsMet = EvaluateAndFoldConditions(executable.SetConditions[setIdx], context);

                // Skip all systems that belong to this set, not just the current one
                if (!setConditionsMet) {
                    CompletedSystems.Or(executable.SystemsInSetsWithConditions[setIdx]);
                }

                shouldRun &= setConditionsMet;
                EvaluatedSets.Set(setIdx);
            }

            // Evaluate System's conditions
            var systemConditionsMet =
                EvaluateAndFoldConditions(executable.SystemConditions[systemIndex], context);
            shouldRun &= systemConditionsMet;

            CompletedSystems.Set(systemIndex);
            if (!shouldRun) {
                continue;
            }

            context.RunSystem(executable.Systems[systemIndex]);
        }
    }

    protected bool EvaluateAndFoldConditions(List<ICondition<TContext>> conditions, TContext ctx) {
        // Not short-circuiting is intentional
        var met = true;
        foreach (var condition in conditions) {
            if (!ctx.EvaluateCondition(condition)) {
                met = false;
            }
        }

        return met;
    }
}