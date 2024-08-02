using Flecs.NET.Bindings;
using Flecs.NET.Core;
using PolyECS.Queries;

namespace PolyECS.Systems;

public interface IIntoSystemParam<T>
{
    public static abstract ISystemParam<T> IntoParam(PolyWorld world);
}

public interface ISystemParam<T>
{
    public void Initialize(PolyWorld world, SystemMeta meta);
    public void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration);
    public T Get(PolyWorld world, SystemMeta systemMeta);
}

public abstract class SystemParam<T> : ISystemParam<T>
{
    public abstract void Initialize(PolyWorld world, SystemMeta meta);
    public abstract void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration);
    public abstract T Get(PolyWorld world, SystemMeta systemMeta);
}

public class VoidParam : SystemParam<object?>, IIntoSystemParam<object?>
{
    public override void Initialize(PolyWorld world, SystemMeta meta) { }
    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGen) { }
    public override object Get(PolyWorld world, SystemMeta systemMeta) => null!;

    public static ISystemParam<object?> IntoParam(PolyWorld world)
    {
        return new VoidParam();
    }
}

public static class Param
{
    public static ISystemParam<Query> Of(Query query) => new QueryParam(query);
    public static ISystemParam<PolyWorld> Of(PolyWorld world) => new WorldParam();
    public static ISystemParam<Res<T>> OfRes<T>() => new ResParam<T>();
    public static ISystemParam<ResMut<T>> OfResMut<T>() => new ResMutParam<T>();
    public static ISystemParam<T> Local<T>(T value) => new LocalParam<T>(value);
}

public class ResParam<T> : SystemParam<Res<T>>
{
    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        unsafe
        {
            world.RegisterResource<T>();
            _globalEntity = world.Entity<T>();
            meta.ComponentAccessSet.AddUnfilteredRead(Type<T>.Id(world.World.Handle));
        }
    }

    private Entity _globalEntity;

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration)
    {
        if (table.Equals(_globalEntity.Table()))
        {
            unsafe
            {
                meta.TableComponentAccess.AddRead(new TableComponentId(tableGeneration, Type<T>.Id(_globalEntity.World)));
            }
        }
    }

    public override Res<T> Get(PolyWorld world, SystemMeta systemMeta) => world.GetResource<T>();
}

public class ResMutParam<T> : SystemParam<ResMut<T>>
{
    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        unsafe
        {
            world.RegisterResource<T>();
            _globalEntity = world.World.Entity<T>();
            meta.ComponentAccessSet.AddUnfilteredWrite(Type<T>.Id(world.World.Handle));
        }
    }

    private Entity _globalEntity;

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration)
    {
        if (table.Equals(_globalEntity.Table()))
        {
            unsafe
            {
                meta.TableComponentAccess.AddWrite(new TableComponentId(tableGeneration, Type<T>.Id(_globalEntity.World)));
            }
        }
    }

    public override ResMut<T> Get(PolyWorld world, SystemMeta systemMeta) => world.GetResourceMut<T>();
}

/// <summary>
/// A wrapping <see cref="ISystemParam{T}"/> that has callbacks for initialization, evaluation and getting the value. Each hook will be called after the
/// wrapped <see cref="ISystemParam{T}"/> has been called.
/// </summary>
public class HookedParam<T>(ISystemParam<T> param, HookedParam<T>.OnInit? init, HookedParam<T>.OnEval? eval, HookedParam<T>.OnGet? get)
    : SystemParam<T>
{
    public delegate void OnInit(PolyWorld world, SystemMeta meta);
    public delegate void OnEval(SystemMeta meta, Table table, int tableGen);
    public delegate T OnGet(PolyWorld world, SystemMeta systemMeta, T childT);

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        param.Initialize(world, meta);
        init?.Invoke(world, meta);
    }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration)
    {
        param.EvaluateNewTable(meta, table, tableGeneration);
        eval?.Invoke(meta, table, tableGeneration);
    }

    public override T Get(PolyWorld world, SystemMeta systemMeta)
    {
        var value = param.Get(world, systemMeta);
        if (get == null)
        {
            return value;
        }
        return get.Invoke(world, systemMeta, value);
    }
}

public class WriteAnnotatedParam<T> : SystemParam<T>
{
    private ISystemParam<T> _param;

    public WriteAnnotatedParam(ISystemParam<T> param, IEnumerable<ulong> writes)
    {
        Writes = writes;
        _param = param;
    }

    protected IEnumerable<ulong> Writes;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        _param.Initialize(world, meta);
        foreach (var write in Writes)
        {
            if (!meta.ComponentAccessSet.UpgradeReadToWrite(write))
            {
                throw new InvalidOperationException($"WriteAnnotatedParam: Component {write} is not read.");
            }
        }
    }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration)
    {
        foreach (var write in Writes)
        {
            if (table.Has(write))
            {
                meta.TableComponentAccess.AddWrite(new TableComponentId(tableGeneration, write));
            }
        }
    }

    public override T Get(PolyWorld world, SystemMeta systemMeta) => throw new NotImplementedException();
}

public class WorldParam : SystemParam<PolyWorld>
{
    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        meta.ComponentAccessSet.WriteAll();
        meta.TableComponentAccess.WriteAll();
        meta.HasDeferred = true;
        _world = world;
    }

    private PolyWorld _world;

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration) { }

    public override PolyWorld Get(PolyWorld world, SystemMeta systemMeta) => _world;

    public static implicit operator WorldParam(PolyWorld world)
    {
        return new WorldParam();
    }
}

public class TQueryParam<TData> : SystemParam<TQuery<TData>>
{
    public TQueryParam(TQuery<TData> query)
    {
        Param = new QueryParam(query.Query);
        Query = query;
    }

    protected TQuery<TData> Query;
    protected readonly QueryParam Param;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        Param.Initialize(world, meta);
    }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration)
    {
        Param.EvaluateNewTable(meta, table, tableGeneration);
    }

    public override TQuery<TData> Get(PolyWorld world, SystemMeta systemMeta) => Query;
}

public class TQueryParam<TData, TFilter> : SystemParam<TQuery<TData, TFilter>>
{
    public TQueryParam(TQuery<TData, TFilter> query)
    {
        Param = new QueryParam(query.Query);
        Query = query;
    }

    protected TQuery<TData, TFilter> Query;
    protected readonly QueryParam Param;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        Param.Initialize(world, meta);
    }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration)
    {
        Param.EvaluateNewTable(meta, table, tableGeneration);
    }

    public override TQuery<TData, TFilter> Get(PolyWorld world, SystemMeta systemMeta)
    {
        return Query;
    }
}

public class QueryParam : SystemParam<Query>
{
    public QueryParam(Query query) => _query = query;
    private readonly Query _query;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        unsafe
        {
            if (_query.World != world.World)
            {
                throw new InvalidOperationException("Query was created with a different world than the one it is being used with.");
            }
        }
        var access = new FilteredAccess<ulong>();
        for (int i = 0; i < _query.TermCount(); i++)
        {
            var term = _query.Term(i);
            switch (term.Oper())
            {
                case flecs.ecs_oper_kind_t.EcsNot:
                    access.AndWithout(term.Id());
                    break;

                case flecs.ecs_oper_kind_t.EcsAnd:
                    access.AndWith(term.Id());
                    AddTermAccess(term, access);
                    break;

                case flecs.ecs_oper_kind_t.EcsOptional:
                    // Add to read but don't add a With/Required filter. 
                    AddTermAccess(term, access);
                    break;

                default:
                    // I'm going to ignore AndFrom/prefab behavior for now. If this comes up i'll add it.
                    throw new InvalidOperationException($"Unknown query term oper: {term.Oper()}");
            }
        }
        meta.ComponentAccessSet.Add(access);
    }

    private void AddTermAccess(Term term, FilteredAccess<ulong> access)
    {
        switch (term.InOut())
        {
            case flecs.ecs_inout_kind_t.EcsIn:
                access.AddRead(term.Id());
                break;

            case flecs.ecs_inout_kind_t.EcsInOut:
            case flecs.ecs_inout_kind_t.EcsInOutDefault:
                access.AddRead(term.Id());
                access.AddWrite(term.Id());
                break;

            case flecs.ecs_inout_kind_t.EcsOut:
                access.AddWrite(term.Id());
                break;
        }
    }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGen)
    {
        var tableTypes = table.Type();
        foreach (var filter in meta.ComponentAccessSet.FilteredAccesses)
        {
            if (!filter.WithFilters().All(table.Has))
            {
                continue;
            }
            if (filter.WithoutFilters().Any(table.Has))
            {
                continue;
            }

            if (filter.Access.WritesAll)
            {
                meta.TableComponentAccess.WriteAll();
                // This is maximally permissive, so we can skip the rest of the filters.
                return;
            }
            if (filter.Access.ReadsAll)
            {
                meta.TableComponentAccess.ReadAll();
            }
            foreach (var read in filter.Access.ReadsAndWrites)
            {
                if (table.Has(read))
                {
                    meta.TableComponentAccess.AddRead(new TableComponentId(tableGen, read));
                }
            }
            foreach (var write in filter.Access.Writes)
            {
                if (table.Has(write))
                {
                    meta.TableComponentAccess.AddWrite(new TableComponentId(tableGen, write));
                }
            }
        }
    }

    public override Query Get(PolyWorld world, SystemMeta systemMeta) => _query;

    public static implicit operator QueryParam(Query query)
    {
        return new QueryParam(query);
    }
}

public class LocalParam<T> : SystemParam<T>
{
    protected T Value;

    public LocalParam()
    {
        Value = default;
    }

    public LocalParam(T value)
    {
        Value = value;
    }

    public override void Initialize(PolyWorld world, SystemMeta meta) { }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration) { }

    public override T Get(PolyWorld world, SystemMeta systemMeta) => Value;
}

/// <summary>
/// A simple parameter that access a world singleton. <see cref="Res"/> and <see cref="ResMut{T}"/> should be used in actual applications. 
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonParam<T> : SystemParam<Ref<T>>
{
    protected ulong _id;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        unsafe
        {
            _id = Type<T>.Id(world.World);
            meta.ComponentAccessSet.AddUnfilteredWrite(_id);
        }
    }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGen)
    {
        if (table.Has(_id))
        {
            meta.TableComponentAccess.AddWrite(new TableComponentId(tableGen, _id));
        }
    }

    public override Ref<T> Get(PolyWorld world, SystemMeta systemMeta) => world.World.GetRef<T>();
}

public class BiParam<T1, T2> : SystemParam<(T1, T2)>
{
    public BiParam(ISystemParam<T1> p1, ISystemParam<T2> p2) => _params = (p1, p2);
    private readonly (ISystemParam<T1>, ISystemParam<T2>) _params;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        _params.Item1.Initialize(world, meta);
        _params.Item2.Initialize(world, meta);
    }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGen)
    {
        _params.Item1.EvaluateNewTable(meta, table, tableGen);
        _params.Item2.EvaluateNewTable(meta, table, tableGen);
    }

    public override (T1, T2) Get(PolyWorld world, SystemMeta systemMeta)
    {
        return (_params.Item1.Get(world, systemMeta), _params.Item2.Get(world, systemMeta));
    }

    public static implicit operator BiParam<T1, T2>((ISystemParam<T1>, ISystemParam<T2>) tuple)
    {
        return new BiParam<T1, T2>(tuple.Item1, tuple.Item2);
    }
}

public class TriParam<T1, T2, T3> : SystemParam<(T1, T2, T3)>
{
    public TriParam(ISystemParam<T1> p1, ISystemParam<T2> p2, ISystemParam<T3> p3) => _params = (p1, p2, p3);
    private readonly (ISystemParam<T1>, ISystemParam<T2>, ISystemParam<T3>) _params;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        _params.Item1.Initialize(world, meta);
        _params.Item2.Initialize(world, meta);
        _params.Item3.Initialize(world, meta);
    }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGen)
    {
        _params.Item1.EvaluateNewTable(meta, table, tableGen);
        _params.Item2.EvaluateNewTable(meta, table, tableGen);
        _params.Item3.EvaluateNewTable(meta, table, tableGen);
    }

    public override (T1, T2, T3) Get(PolyWorld world, SystemMeta systemMeta)
    {
        return (_params.Item1.Get(world, systemMeta), _params.Item2.Get(world, systemMeta), _params.Item3.Get(world, systemMeta));
    }
}
