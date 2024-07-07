using Flecs.NET.Core;
using PolyECS.Systems;

namespace PolyECS;

/// <summary>
/// A wrapper around <see cref="World"/> with some helper methods.
/// </summary>
public partial class PolyWorld : IDisposable
{
    public PolyWorld(World world)
    {
        World = world;
    }

    public PolyWorld()
    {
        World = World.Create();
        TableCache = new TableCache(World);
        TableCache.Update();
    }

    public bool DeferBegin()
    {
        return World.DeferBegin();
    }

    public bool DeferEnd()
    {
        if (!World.DeferEnd())
        {
            return false;
        }
        TableCache.Update();
        return true;
    }

    public World World { get; private set; }
    public TableCache TableCache { get; private set; }

    public void RunSystemOnce<T>(T system) where T : RunSystem
    {
        if (!World.IsDeferred())
        {
            TableCache.Update();
        }
        system.Initialize(this);
        system.UpdateTableComponentAccess(TableCache);
        system.Run(null, this);
    }

    public void RunSystem<T>(T system) where T : RunSystem
    {
        system.UpdateTableComponentAccess(TableCache);
        system.Run(null, this);
    }

    public void AddResource<T>(T resource)
    {
        World.Set<T>(resource);
    }

    public void Dispose()
    {
        World.Dispose();
    }
}
