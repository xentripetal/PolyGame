using Flecs.NET.Core;
using PolyECS.Systems.Graph;

namespace PolyECS.Systems.Executor;

public interface IExecutor
{
    void Init(SystemSchedule schedule);
    void SetApplyFinalDeferred(bool apply);
    void Run(SystemSchedule executable, World world, FixedBitSet? skipSystems);
}
