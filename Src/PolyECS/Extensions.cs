using Flecs.NET.Core;
using PolyECS.Systems;

namespace PolyECS;

public static class Extensions
{
    public static ISystemParam<Query> AsParam(this Query query)
    {
        return new QueryParam(query);
    }
    
    public static ISystemParam<(Query, Query)> AsParam(this (Query, Query) queries)
    {
        return new BiParam<Query, Query>(queries.Item1.AsParam(), queries.Item2.AsParam());
    }
    
    public static ISystemParam<(Query, Query, Query)> AsParam(this (Query, Query, Query) queries)
    {
        return new TriParam<Query, Query, Query>(queries.Item1.AsParam(), queries.Item2.AsParam(), queries.Item3.AsParam());
    }
}
