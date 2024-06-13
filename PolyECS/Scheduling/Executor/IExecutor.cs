using PolyECS.Systems.Graph;

namespace PolyECS.Systems.Executor;

public interface IExecutor<T>
{
    void Init(SystemSchedule<T> schedule);
    void SetApplyFinalDeferred(bool apply);
    void Run(SystemSchedule<T> executable, IScheduleWorld world, FixedBitSet? skipSystems);
}
