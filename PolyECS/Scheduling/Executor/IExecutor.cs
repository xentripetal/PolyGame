using PolyFlecs.Systems.Graph;

namespace PolyFlecs.Systems.Executor;

public interface IExecutor
{
    void Init(SystemSchedule schedule);
    void SetApplyFinalDeferred(bool apply);
    void Run(SystemSchedule executable, IScheduleWorld world, FixedBitSet? skipSystems);
}
