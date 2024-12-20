using Flecs.NET.Bindings;
using Flecs.NET.Core;
using PolyECS.Systems;

namespace PolyECS;

/// <summary>
/// </summary>
public partial class SystemBuilder
{
    protected PolyWorld World;

    protected List<ISystemParamMetadata> ParamMetadatas = new ();
    protected SystemMeta Meta;

    public Res<T> WithRes<T>()
    {
        ParamMetadatas.Add(new ResParamMetadata<T>(World, Meta));
        return World.GetResource<T>();
    }

    public ResMut<T> WithResMut<T>()
    {
        ParamMetadatas.Add(new ResMutParamMetadata<T>(World, Meta));
        return World.GetResourceMut<T>();
    }

    public Query<T> Query<T>()
    {
        var q = World.Query<T>();
        unsafe
        {
            ParamMetadatas.Add(new QueryParam(new Query(q.Handle), Meta));
        }
        return q;
    }

    public Query<T> Query<T>(Func<QueryBuilder<T>, QueryBuilder<T>> qb)
    {
        var q = qb(World.World.QueryBuilder<T>()).Build();
        unsafe
        {
            ParamMetadatas.Add(new QueryParam(new Query(q.Handle), Meta));
        }
        return q;
    }

    public Query Query(Func<QueryBuilder, QueryBuilder> qb)
    {
        var q = qb(World.QueryBuilder()).Build();
        unsafe
        {
            ParamMetadatas.Add(new QueryParam(new Query(q.Handle), Meta));
        }
        return q;
    }
}