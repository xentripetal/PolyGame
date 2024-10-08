using Flecs.NET.Core;
using PolyECS.Systems;

namespace PolyECS.Queries;

public static class QueryHelpers
{
    public static QueryBuilder ApplyData<T>(QueryBuilder qb)
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
        return qb.With<T>();
    }
}

/// <summary>
///     A typed query for use in systems without having to manually build the query.
/// </summary>
/// <typeparam name="TData"></typeparam>
/// <typeparam name="TFilter"></typeparam>
public class TQuery<TData, TFilter> : IIntoSystemParam<TQuery<TData, TFilter>> where TFilter : IIntoFilter
{
    public TQuery(PolyWorld world) => Query = Build(world);

    internal TQuery(Query query) => Query = query;

    public Query Query { get; private set; }

    public static ITSystemParam<TQuery<TData, TFilter>> IntoParam(PolyWorld world) => new TQueryParam<TData, TFilter>(new TQuery<TData, TFilter>(world));

    public static Query Build(PolyWorld world)
    {
        var qb = world.QueryBuilder();
        qb = QueryHelpers.ApplyData<TData>(qb);
        qb = TFilter.ApplyFilter(qb);
        return qb.Build();
    }

    protected static QueryBuilder ApplyData(QueryBuilder qb)
    {
        var data = default(TData);
        if (data is IIntoData intoData)
        {
            if (data == null)
            {
                throw new NotSupportedException($"IIntoData implementor ({typeof(TData)}) cannot be a reference type");
            }
            return intoData.ApplyData(qb);
        }
        return qb.With<TData>();
    }
}

public class TQuery<TData> : TQuery<TData, VoidFilter>, IIntoSystemParam<TQuery<TData>>
{
    public TQuery(PolyWorld world) : base(world) { }

    public new static ITSystemParam<TQuery<TData>> IntoParam(PolyWorld world) => new TQueryParam<TData>(new TQuery<TData>(world));
}

public interface IIntoData
{
    /// <summary>
    ///     Expands any data requirements into the query builder. Not static since we
    /// </summary>
    /// <param name="qb"></param>
    /// <returns></returns>
    public QueryBuilder ApplyData(QueryBuilder qb);
}

public interface IIntoFilter
{
    public abstract static QueryBuilder ApplyFilter(QueryBuilder qb);
}

public struct Components<T> : IIntoData
{
    public QueryBuilder ApplyData(QueryBuilder qb) => QueryHelpers.ApplyData<T>(qb);
}

public struct Components<T1, T2> : IIntoData
{
    public QueryBuilder ApplyData(QueryBuilder qb)
    {
        qb = QueryHelpers.ApplyData<T1>(qb);
        return QueryHelpers.ApplyData<T2>(qb);
    }
}

public struct Components<T1, T2, T3> : IIntoData
{
    public QueryBuilder ApplyData(QueryBuilder qb)
    {
        qb = QueryHelpers.ApplyData<T1>(qb);
        qb = QueryHelpers.ApplyData<T2>(qb);
        return QueryHelpers.ApplyData<T2>(qb);
    }
}

public struct Components<T1, T2, T3, T4> : IIntoData
{
    public QueryBuilder ApplyData(QueryBuilder qb)
    {
        qb = QueryHelpers.ApplyData<T1>(qb);
        qb = QueryHelpers.ApplyData<T2>(qb);
        qb = QueryHelpers.ApplyData<T3>(qb);
        return QueryHelpers.ApplyData<T4>(qb);
    }
}

public struct R<T> : IIntoData
{
    public QueryBuilder ApplyData(QueryBuilder qb) => QueryHelpers.ApplyData<T>(qb).In();
}

public struct RW<T> : IIntoData
{
    public QueryBuilder ApplyData(QueryBuilder qb) => QueryHelpers.ApplyData<T>(qb).InOut();
}

public struct W<T> : IIntoData
{
    public QueryBuilder ApplyData(QueryBuilder qb) => QueryHelpers.ApplyData<T>(qb).Out();
}

public struct VoidFilter : IIntoFilter
{
    public static QueryBuilder ApplyFilter(QueryBuilder qb) => qb;
}

public struct With<T> : IIntoFilter
{
    public static QueryBuilder ApplyFilter(QueryBuilder qb) => qb.With<T>().InOutNone();
}

public struct Without<T> : IIntoFilter
{
    public static QueryBuilder ApplyFilter(QueryBuilder qb) => qb.Without<T>();
}
