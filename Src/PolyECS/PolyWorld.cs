using DotNext;
using Flecs.NET.Core;
using PolyECS.Scheduling;
using PolyECS.Systems;
using Serilog;

namespace PolyECS;

/// <summary>
///     A wrapper around <see cref="FlecsWorld" /> with some helper methods.
/// </summary>
public partial class PolyWorld : IDisposable, IIntoSystemParam, IStaticSystemParam<PolyWorld>
{
    public PolyWorld(World flecsWorld)
    {
        FlecsWorld = flecsWorld;
        TableCache = new TableCache(FlecsWorld);
        SetResource(new ScheduleContainer());
        SetResource(TableCache);
        TableCache.Update();
    }

    public PolyWorld()
    {
        FlecsWorld = World.Create();
        TableCache = new TableCache(FlecsWorld);
        SetResource(new ScheduleContainer());
        SetResource(TableCache);
        TableCache.Update();
    }

    public World FlecsWorld { get; }
    public TableCache TableCache { get; }
    
    public readonly ResourceStorage Resources = new ();

    public void Dispose()
    {
        FlecsWorld.Dispose();
    }

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
        var schedules = MustGetResource<ScheduleContainer>();
        var schedule = schedules.Remove(label);
        if (schedule == null)
        {
            return new Result<T>(new ArgumentException($"Schedule for label {label} not found"));
        }
        var data = fn(this, schedule);
        var old = schedules.Insert(schedule);
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


    public bool DeferBegin() => FlecsWorld.DeferBegin();

    public bool DeferEnd()
    {
        if (!FlecsWorld.DeferEnd())
        {
            return false;
        }
        TableCache.Update();
        return true;
    }

    public void RunSystemOnce<T>(T system) where T : ISystem
    {
        if (!FlecsWorld.IsDeferred())
        {
            TableCache.Update();
        }
        system.Initialize(this);
        system.UpdateStorageAccess(TableCache, Resources);
        system.TryRun(this);
    }

    public void RunSystem<T>(T system) where T : ISystem
    {
        system.UpdateStorageAccess(TableCache, Resources);
        system.TryRun(this);
    }

    public void SetResource<T>(T resource)
    {
        Resources.Set(resource);
    }

    public int RegisterResource<T>()
    {
        return Resources.Register<T>();
    }
    
    public int RegisterResource(Type t)
    {
        return Resources.Register(t);
    }
    

    public Component<T> Register<T>()
    {
        unsafe
        {
            return new Component<T>(FlecsWorld.Handle, Type<T>.RegisterComponent(FlecsWorld, true, true, 0, ""));
        }
    }

    public void RegisterComponent<T>() where T : IComponent
    {
        var c = Register<T>();
        T.Register(c.UntypedComponent);
    }

    public Res<T> GetResource<T>() => new (this);
    public ResMut<T> GetResourceMut<T>() => new (this);

    public T MustGetResource<T>()
    {
        var ok = Resources.TryGet<T>(out var value);
        if (!ok) throw new InvalidOperationException($"Resource {typeof(T)} not found");
        return value!;
    }


    #region Flecs.World Proxies

    public void Set<T>(T value) => FlecsWorld.Set(value);
    public T Get<T>() => FlecsWorld.Get<T>();
    public Entity Entity() => FlecsWorld.Entity();

    public Entity Entity(string name) => FlecsWorld.Entity(name);

    public Entity Entity(ulong id) => FlecsWorld.Entity(id);

    public Entity Entity<T>() => FlecsWorld.Entity<T>();

    public Entity Entity<T>(string name) => FlecsWorld.Entity<T>(name);

    public QueryBuilder QueryBuilder() => FlecsWorld.QueryBuilder();
    public QueryBuilder<T>  QueryBuilder<T>() => FlecsWorld.QueryBuilder<T>();
    
    public QueryBuilder<T1, T2> QueryBuilder<T1, T2>() => FlecsWorld.QueryBuilder<T1, T2>();
    
    public QueryBuilder<T1, T2, T3> QueryBuilder<T1, T2, T3>() => FlecsWorld.QueryBuilder<T1, T2, T3>();
    
    public QueryBuilder<T1, T2, T3, T4> QueryBuilder<T1, T2, T3, T4>() => FlecsWorld.QueryBuilder<T1, T2, T3, T4>();
    
    public QueryBuilder<T1, T2, T3, T4, T5> QueryBuilder<T1, T2, T3, T4, T5>() => FlecsWorld.QueryBuilder<T1, T2, T3, T4, T5>();
    
    public QueryBuilder<T1, T2, T3, T4, T5, T6> QueryBuilder<T1, T2, T3, T4, T5, T6>() => FlecsWorld.QueryBuilder<T1, T2, T3, T4, T5, T6>();
    
    public Query<T> Query<T>() => FlecsWorld.Query<T>();

    public Query<T1, T2> Query<T1, T2>() => FlecsWorld.Query<T1, T2>();

    public Query<T1, T2, T3> Query<T1, T2, T3>() => FlecsWorld.Query<T1, T2, T3>();

    public Query<T1, T2, T3, T4> Query<T1, T2, T3, T4>() => FlecsWorld.Query<T1, T2, T3, T4>();

    public Query<T1, T2, T3, T4, T5> Query<T1, T2, T3, T4, T5>() => FlecsWorld.Query<T1, T2, T3, T4, T5>();

    public Query<T1, T2, T3, T4, T5, T6> Query<T1, T2, T3, T4, T5, T6>() => FlecsWorld.Query<T1, T2, T3, T4, T5, T6>();

    #endregion

    public ISystemParam IntoParam(PolyWorld world) => new PolyWorldParam();
    public static PolyWorld BuildParamValue(PolyWorld world)
    {
        return world;
    }

    public static ISystemParam GetParam(PolyWorld world, PolyWorld value)
    {
        return new PolyWorldParam();
    }

    public ParamBuilder GetParamBuilder()
    {
        return new ParamBuilder(this);
    }
}
