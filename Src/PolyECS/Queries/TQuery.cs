using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Flecs.NET.Core;
using PolyECS.Systems;

namespace PolyECS.Queries;

/// <summary>
/// A typed query for use in systems without having to manually build the query. 
/// </summary>
/// <typeparam name="TData"></typeparam>
/// <typeparam name="TFilter"></typeparam>
public class TQuery<TData, TFilter> : IIntoSystemParam<TQuery<TData, TFilter>>
{
    public TQuery(PolyWorld world)
    {
        Query = Build(world);
    }
    
    internal TQuery(Query query)
    {
        Query = query;
    }

    public Query Query { get; private set; }

    public static Query Build(PolyWorld world)
    {
        var qb = world.QueryBuilder();
        qb = ApplyData<TData>(qb);
        qb = ApplyFilter<TFilter>(qb);
        return qb.Build();
    }

    protected static QueryBuilder ApplyData<T>(QueryBuilder qb)
    {
        var data = default(T);
        if (data is IIntoData intoData)
        {
            if (data == null)
            {
                throw new NotSupportedException($"IIntoData implementor ({typeof(T)}) cannot be a reference type");
            }
            return intoData.ApplyData(qb);
        }
        if (typeof(ITuple).IsAssignableFrom(typeof(T)))
        {
            foreach (var t in typeof(T).GetGenericArguments())
            {
                // TODO verify if this will break AoT
                qb = (QueryBuilder)typeof(TQuery<TData, TFilter>).GetMethod("ApplyData")!.MakeGenericMethod(t).Invoke(null, [qb]);
            }
            return qb;
        }
        return qb.With<T>();
    }

    protected static QueryBuilder ApplyFilter<T>(QueryBuilder qb)
    {
        return qb;
    }

    public static ISystemParam<TQuery<TData, TFilter>> IntoParam(PolyWorld world)
    {
        return new TQueryParam<TData, TFilter>(new TQuery<TData, TFilter>(world));
    }
}

public class TQuery<TData> : TQuery<TData, VoidFilter>, IIntoSystemParam<TQuery<TData>> 
{
    public TQuery(PolyWorld world) : base(world) { }

    public static ISystemParam<TQuery<TData>> IntoParam(PolyWorld world)
    {
        return new TQueryParam<TData>(new TQuery<TData>(world));
    }
}

public interface IIntoData
{
    public QueryBuilder ApplyData(QueryBuilder qb);
}

public interface IIntoFilter
{
    public QueryBuilder ApplyFilter(QueryBuilder qb);
}

public struct R<T> : IIntoData
{
    public QueryBuilder ApplyData(QueryBuilder qb)
    {
        return qb.With<T>().In();
    }
}

public struct RW<T> : IIntoData
{
    public QueryBuilder ApplyData(QueryBuilder qb)
    {
        return qb.With<T>().InOut();
    }
}

public struct W<T> : IIntoData
{
    public QueryBuilder ApplyData(QueryBuilder qb)
    {
        return qb.With<T>().Out();
    }
}


public struct VoidFilter : IIntoFilter
{
    public QueryBuilder ApplyFilter(QueryBuilder qb) => qb;
}

public struct With<T> : IIntoFilter
{
    public QueryBuilder ApplyFilter(QueryBuilder qb)
    {
        return qb.With<T>().InOutNone();
    }
}

public struct Without<T> : IIntoFilter
{
    public QueryBuilder ApplyFilter(QueryBuilder qb)
    {
        return qb.Without<T>();
    }
}
