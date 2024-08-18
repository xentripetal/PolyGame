namespace PolyECS.Scheduling.Executor;

public interface IExecutor
{
    void Init(SystemSchedule schedule);
    void SetApplyFinalDeferred(bool apply);
    void Run(SystemSchedule executable, PolyWorld world, FixedBitSet? skipSystems);
}
