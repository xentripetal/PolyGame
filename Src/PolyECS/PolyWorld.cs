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

    public void SetResource<T>(T resource)
    {
        World.Set<T>(resource);
    }

    public void RegisterResource<T>()
    {
        World.Add<T>();
    }

    public void Dispose()
    {
        World.Dispose();
    }

    public void Register<T>()
    {
        Type<T>.RegisterComponent(World, true, true, 0, "");
    }

    public void RegisterComponent<T>() where T : IComponent
    {
        unsafe
        {
            var id = Type<T>.RegisterComponent(World, true, true, 0, "");
            T.Register(new UntypedComponent(World.Handle, id));
        }
    }

    public Res<T> GetResource<T>() => new Res<T>(World);
    public ResMut<T> GetResourceMut<T>() => new ResMut<T>(World);


    #region Flecs.World Proxies

    public void Set<T>(T value) => World.Set(value);
    public T Get<T>() => World.Get<T>();
    public Entity Entity() => World.Entity();

    public Entity Entity(string name) => World.Entity(name);

    public Entity Entity(ulong id) => World.Entity(id);

    public Entity Entity<T>() => World.Entity<T>();

    public Entity Entity<T>(string name) => World.Entity<T>(name);

    public QueryBuilder QueryBuilder() => World.QueryBuilder();

    public Query Query<T>() => World.Query<T>();

    public Query Query<T1, T2>() => World.Query<T1, T2>();

    public Query Query<T1, T2, T3>() => World.Query<T1, T2, T3>();

    public Query Query<T1, T2, T3, T4>() => World.Query<T1, T2, T3, T4>();

    public Query Query<T1, T2, T3, T4, T5>() => World.Query<T1, T2, T3, T4, T5>();

    public Query Query<T1, T2, T3, T4, T5, T6>() => World.Query<T1, T2, T3, T4, T5, T6>();

    #endregion
}
