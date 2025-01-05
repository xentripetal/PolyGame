using Flecs.NET.Core;

namespace PolyECS.Systems;

public partial class ParamBuilder
{
    
    public ParamBuilder(PolyWorld world)
    {
        World = world;
    }
    
    /// <summary>
    /// Direct access to the world. Use with caution, any parameters created from this won't be included in the final build.
    /// </summary>
    public PolyWorld World { get; }

    protected List<ISystemParam> Params { get; } = new();

    public Res<T> Res<T>()
    {
        var res = new Res<T>(World);
        Params.Add(res.IntoParam(World));
        return res;
    }

    public ResMut<T> ResMut<T>()
    {
        var res = new ResMut<T>(World);
        Params.Add(res.IntoParam(World));
        return res;
    }
    
    public unsafe Query<T> QueryBuilder<T>(Func<QueryBuilder<T>, QueryBuilder<T>> builder)
    {
        var query = builder(World.FlecsWorld.QueryBuilder<T>()).Build();
        Params.Add(new QueryParam(new Query(query.Handle)));
        return query;
    }
    
    public unsafe Query<T1, T2> QueryBuilder<T1, T2>(Func<QueryBuilder<T1, T2>, QueryBuilder<T1, T2>> builder)
    {
        var query = builder(World.FlecsWorld.QueryBuilder<T1, T2>()).Build();
        Params.Add(new QueryParam(new Query(query.Handle)));
        return query;
    }
    
    public unsafe Query<T1, T2, T3> QueryBuilder<T1, T2, T3>(Func<QueryBuilder<T1, T2, T3>, QueryBuilder<T1, T2, T3>> builder)
    {
        var query = builder(World.FlecsWorld.QueryBuilder<T1, T2, T3>()).Build();
        Params.Add(new QueryParam(new Query(query.Handle)));
        return query;
    }
    
    public unsafe Query<T1, T2, T3, T4> QueryBuilder<T1, T2, T3, T4>(Func<QueryBuilder<T1, T2, T3, T4>, QueryBuilder<T1, T2, T3, T4>> builder)
    {
        var query = builder(World.FlecsWorld.QueryBuilder<T1, T2, T3, T4>()).Build();
        Params.Add(new QueryParam(new Query(query.Handle)));
        return query;
    }
    
    public unsafe Query<T1, T2, T3, T4, T5> QueryBuilder<T1, T2, T3, T4, T5>(Func<QueryBuilder<T1, T2, T3, T4, T5>, QueryBuilder<T1, T2, T3, T4, T5>> builder)
    {
        var query = builder(World.FlecsWorld.QueryBuilder<T1, T2, T3, T4, T5>()).Build();
        Params.Add(new QueryParam(new Query(query.Handle)));
        return query;
    }
    
    public unsafe Query<T1, T2, T3, T4, T5, T6> QueryBuilder<T1, T2, T3, T4, T5, T6>(Func<QueryBuilder<T1, T2, T3, T4, T5, T6>, QueryBuilder<T1, T2, T3, T4, T5, T6>> builder)
    {
        var query = builder(World.FlecsWorld.QueryBuilder<T1, T2, T3, T4, T5, T6>()).Build();
        Params.Add(new QueryParam(new Query(query.Handle)));
        return query;
    }

    public unsafe Query<T> Query<T>()
    {
        var query = World.FlecsWorld.Query<T>();
        Params.Add(new QueryParam(new Query(query.Handle)));
        return query;
    }
    
    public unsafe Query<T1, T2> Query<T1, T2>()
    {
        var query = World.FlecsWorld.Query<T1, T2>();
        Params.Add(new QueryParam(new Query(query.Handle)));
        return query;
    }
    
    public unsafe Query<T1, T2, T3> Query<T1, T2, T3>()
    {
        var query = World.FlecsWorld.Query<T1, T2, T3>();
        Params.Add(new QueryParam(new Query(query.Handle)));
        return query;
    }
    
    public unsafe Query<T1, T2, T3, T4> Query<T1, T2, T3, T4>()
    {
        var query = World.FlecsWorld.Query<T1, T2, T3, T4>();
        Params.Add(new QueryParam(new Query(query.Handle)));
        return query;
    }
    
    public unsafe Query<T1, T2, T3, T4, T5> Query<T1, T2, T3, T4, T5>()
    {
        var query = World.FlecsWorld.Query<T1, T2, T3, T4, T5>();
        Params.Add(new QueryParam(new Query(query.Handle)));
        return query;
    }

    /// <summary>
    /// Adds a parameter directly to the builder, this should be used when you need to create a custom parameter from the <see cref="World"/>.
    /// </summary>
    public void With(IIntoSystemParam t)
    {
        Params.Add(t.IntoParam(World));
    }
    
    public T With<T>(T t) where T : IIntoSystemParam
    {
        Params.Add(t.IntoParam(World));
        return t;
    }

    public T With<T>() where T : IStaticSystemParam<T>
    {
        var value = T.BuildParamValue(World);
        Params.Add(T.GetParam(World, value));
        return value;
    }

    public ISystemParam[] Build()
    {
        var res = Params.ToArray();
        Params.Clear();
        return res;
    }
}