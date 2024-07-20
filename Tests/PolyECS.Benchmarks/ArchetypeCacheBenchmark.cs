using Flecs.NET.Core;

namespace PolyECS.Benchmarks;

public class ArchetypeCacheBenchmark : IDisposable
{
    public ArchetypeCacheBenchmark()
    {
        _world = World.Create();
        cache = new TableCache(_world);
        cache.Update();
    }

    protected World _world;
    protected TableCache cache;
    
    public void Dispose()
    {
        _world.Dispose();
    }

    [Benchmark]
    public void DryUpdate()
    {
        cache.Update();
    }
}
