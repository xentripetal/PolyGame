using DotNext;
using Flecs.NET.Core;
using PolyECS.Scheduling;
using PolyECS.Systems;
using Serilog;

namespace PolyECS;

/// <summary>
///     A wrapper around <see cref="World" /> with some helper methods.
/// </summary>
public class PolyWorld : IDisposable, IIntoSystemParam<PolyWorld>
{
    public PolyWorld(World world)
    {
        World = world;
        TableCache = new TableCache(World);
        TableCache.Update();
    }

    public PolyWorld()
    {
        World = World.Create();
        TableCache = new TableCache(World);
        SetResource(new ScheduleContainer());
        SetResource(TableCache);
        TableCache.Update();
    }

    public World World { get; }
    public TableCache TableCache { get; }

    public void Dispose()
    {
        World.Dispose();
    }

    public static ITSystemParam<PolyWorld> IntoParam(PolyWorld world) => new PolyWorldParam();

    /// <summary>
    ///     Temporarily removes the schedule associated with label from the <see cref="ScheduleContainer" />, passes it to the
    ///     provided fn, and finally re-adds it
    ///     to the container.
    /// </summary>
    /// <param name="label">Label of schedule to scope</param>
    /// <param name="fn">Function to invoke with the schedule</param>
    /// <typeparam name="T">Return type from the scoped function</typeparam>
    /// <returns>response from fn</returns>
    public Result<T> ScheduleScope<T>(ScheduleLabel label, Func<PolyWorld, Schedule, T> fn)
    {
        var res = GetResourceMut<ScheduleContainer>();
        if (!res.HasValue)
        {
            return new Result<T>(new InvalidOperationException("ScheduleContainer resource not found"));
        }

        var schedule = res.Get().Remove(label);
        if (schedule == null)
        {
            return new Result<T>(new ArgumentException($"Schedule for label {label} not found"));
        }
        var data = fn(this, schedule);
        var old = res.Get().Insert(schedule);
        if (old != null)
        {
            Log.Warning("Schedule {Label} was inserted during a call to PolyWorld.ScheduleScope, its value has been overwritten", label);
        }

        return data;
    }

    /// <summary>
    ///     Runs the <see cref="Schedule" /> associated with the label a single time
    /// </summary>
    /// <param name="label"></param>
    public Result<Empty> RunSchedule(ScheduleLabel label)
    {
        return ScheduleScope<Empty>(label, (world, schedule) => {
            schedule.Run(world);
            return Empty.Instance;
        });
    }


    public bool DeferBegin() => World.DeferBegin();

    public bool DeferEnd()
    {
        if (!World.DeferEnd())
        {
            return false;
        }
        TableCache.Update();
        return true;
    }

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
        World.Set(resource);
    }

    public void RegisterResource<T>()
    {
        World.Add<T>();
    }

    public Component<T> Register<T>()
    {
        unsafe
        {
            return new Component<T>(World.Handle, Type<T>.RegisterComponent(World, true, true, 0, ""));
        }
    }

    public void RegisterComponent<T>() where T : IComponent
    {
        var c = Register<T>();
        T.Register(c.UntypedComponent);
    }

    public Res<T> GetResource<T>() => new (World);
    public ResMut<T> GetResourceMut<T>() => new (World);


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
