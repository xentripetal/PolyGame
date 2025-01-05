using System.Runtime.CompilerServices;
using Flecs.NET.Core;
using PolyECS.Systems;
using NotSupportedException = System.NotSupportedException;

namespace PolyECS;

public static class QueryHelpers
{
    public static ref QueryBuilder NoopNext(ref QueryBuilder qb) => ref qb;

    internal static ref QueryBuilder ApplyFilter<T>(ref QueryBuilder qb, TermStack stack) where T : new()
    {
        return ref ApplyFilter(ref qb, new T(), stack);
    }

    internal static ref QueryBuilder ApplyFilter(ref QueryBuilder qb, object filter, TermStack stack)
    {
        if (filter is ITuple tup)
        {
            for (var i = 0; i < tup.Length; i++)
                ApplyFilterSingle(ref qb, tup[i], stack);
            return ref qb;
        }

        return ref ApplyFilterSingle(ref qb, filter, stack);
    }

    private static ref QueryBuilder ApplyFilterSingle(ref QueryBuilder qb, object? filter, TermStack stack)
    {
        if (filter is not ITermFilter termFilter)
            throw new NotSupportedException($"Unknown filter type {filter?.GetType()}");

        stack.Push(termFilter.Apply);
        var appliedToChildren = false;
        var children = termFilter.GetChildren(ref qb);
        foreach (var child in children)
        {
            appliedToChildren = true;
            qb = ref ApplyFilter(ref qb, child, stack);
        }

        if (!appliedToChildren)
            qb = ref stack.Apply(ref qb);
        stack.Pop();
        return ref qb;
    }
}

internal class TermStack
{
    private List<QueryModifier> _modifiers = new();

    public delegate ref QueryBuilder QueryModifier(ref QueryBuilder query);

    public void Push(QueryModifier modifier) => _modifiers.Add(modifier);

    public void Pop()
    {
        _modifiers.RemoveAt(_modifiers.Count - 1);
    }

    public ref QueryBuilder Apply(ref QueryBuilder qb)
    {
        for (int i = _modifiers.Count - 1; i >= 0; i--)
            qb = ref _modifiers[i](ref qb);
        return ref qb;
    }
}

public class TQuery<T0, TFilter> : IStaticSystemParam<TQuery<T0, TFilter>>, IIntoSystemParam where TFilter : new()
{
    public TQuery(PolyWorld world)
    {
        Query = Build(world);
    }

    public Query<T0> Query;


    public static Query<T0> Build(PolyWorld world)
    {
        var qb = QueryHelpers.ApplyFilter<TFilter>(ref world.QueryBuilder().With<T0>(), new TermStack());
        unsafe
        {
            return new Query<T0>(qb.Build());
        }
    }

    /// <inheritdoc cref="Query{T0}.Each(Ecs.EachRefCallback{T0})"/>
    public void Each(Ecs.EachRefCallback<T0> callback)
    {
        Query.Each(callback);
    }

    /// <inheritdoc cref="Query{T0}.Each(Ecs.EachEntityRefCallback{T0})"/>
    public void Each(Ecs.EachEntityRefCallback<T0> callback)
    {
        Query.Each(callback);
    }

    /// <inheritdoc cref="Query{T0}.Each(Ecs.EachIterRefCallback{T0})"/>
    public void Each(Ecs.EachIterRefCallback<T0> callback)
    {
        Query.Each(callback);
    }

    /// <inheritdoc cref="Query{T0}.Count"/>
    public int Count() => Query.Count();

    public static TQuery<T0, TFilter> BuildParamValue(PolyWorld world) => new(world);

    public static ISystemParam GetParam(PolyWorld world, TQuery<T0, TFilter> value) => value.IntoParam(world);

    public unsafe ISystemParam IntoParam(PolyWorld world)
    {
        return new QueryParam(new Query(Query.Handle));
    }
}

public delegate ref QueryBuilder QueryModifier(ref QueryBuilder query);

public interface ITermFilter
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb);
    public ref QueryBuilder Apply(ref QueryBuilder qb);
}

public interface ITermSelector : ITermFilter
{
    /// <summary>
    /// Reports whether <see cref="ITermFilter.Apply"/> can be called again to re-apply parent conditions to another
    /// term.
    /// </summary>
    /// <returns></returns>
    public bool HasNext(ref QueryBuilder qb);
}

/// <summary>
/// Marks a component as read only
/// </summary>
/// <typeparam name="T"></typeparam>
public struct In<T> : ITermFilter where T : struct
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [new T()];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.In();
}

/// <summary>
/// Marks a component as both read and write
/// </summary>
/// <typeparam name="T"></typeparam>
public struct InOut<T> : ITermFilter where T : struct
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [new T()];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.InOut();
}

/// <summary>
/// Marks a component as only being written
/// </summary>
/// <typeparam name="T"></typeparam>
public struct Out<T> : ITermFilter where T : struct
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [new T()];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.Out();
}

/// <summary>
/// Marks a component as neither being read and writen
/// </summary>
/// <typeparam name="T"></typeparam>
public struct InOutNone<T> : ITermFilter where T : struct
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [new T()];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.InOutNone();
}

public struct TermAt<T> : ITermFilter
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.TermAt<T>();
}

public struct Term0 : ITermFilter
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.TermAt(0);
}

/// <summary>
/// Reference to all terms currently known to the query. This allows you to easily define readonly queries by setting
/// their filter to something like R{AllTerms}. Remember that TFilters are applied in order, so if you create a new term
/// after using <see cref="AllTerms"/>, it will not apply the filter to your new term.
/// e.g. TQuery{T0, T1, (R{AllTerms}, With{T2})} Will only mark T0 and T1 as read only and T2 will be read/write.
/// </summary>
public struct AllTerms : ITermFilter
{
    internal struct IndexedTerm(int index) : ITermFilter
    {
        public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [];

        public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.TermAt(index);
    }
    
    public IEnumerable<object> GetChildren(ref QueryBuilder qb)
    {
        int termCount = qb.GetTermCount();
        var terms = new List<object>(termCount);
        for (var i = 0; i < termCount; i++)
            terms.Add(new IndexedTerm(i));
        return terms;
    }

    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb;
}

/// <summary>
/// Marks a component as write only.
/// </summary>
/// <typeparam name="T"></typeparam>
public struct W<T> : ITermFilter where T : struct
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [new T()];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.Out();
}

public struct Optional<T> : ITermFilter where T : struct
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [new T()];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.Optional();
}

public struct Cached : ITermFilter
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.Cached();
}

public struct VoidFilter : ITermFilter
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb;
}

public struct Cascade<T> : ITermFilter where T : struct
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [new T()];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.Cascade();
}

public struct Parent<T> : ITermFilter where T : struct
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [new T()];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.Parent();
}

public struct Up<T> : ITermFilter where T : struct
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [new T()];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.Up();
}

/// <summary>
/// Filter to restrict results to entities with a given component. This component will not be included in the result. Use the Data term to include the component.
/// </summary>
/// <typeparam name="T"></typeparam>
public struct With<T> : ITermFilter
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.With<T>();
}

/// <summary>
/// Filter to exclude entities with a given component.
/// </summary>
/// <typeparam name="T"></typeparam>
public struct Without<T> : ITermFilter
{
    public IEnumerable<object> GetChildren(ref QueryBuilder qb) => [];
    public ref QueryBuilder Apply(ref QueryBuilder qb) => ref qb.Without<T>();
}