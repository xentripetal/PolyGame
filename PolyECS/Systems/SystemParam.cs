using Flecs.NET.Bindings;
using Flecs.NET.Core;

namespace PolyECS.Systems;

public interface ISystemParam<T>
{
    public void Initialize(PolyWorld world, SystemMeta meta);
    public void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration);
    public T Get(PolyWorld world, SystemMeta systemMeta);
}

public class VoidParam : ISystemParam<object?>
{
    public void Initialize(PolyWorld world, SystemMeta meta) { }
    public void EvaluateNewTable(SystemMeta meta, Table table, int tableGen) { }
    public object Get(PolyWorld world, SystemMeta systemMeta) => null;
}

/// <summary>
/// A wrapping <see cref="ISystemParam{T}"/> that has callbacks for initialization, evaluation and getting the value. Each hook will be called after the
/// wrapped <see cref="ISystemParam{T}"/> has been called.
/// </summary>
public class HookedParam<T> : ISystemParam<T>
{
    public delegate void OnInit(PolyWorld world, SystemMeta meta);
    public delegate void OnEval(SystemMeta meta, Table table, int tableGen);
    public delegate T OnGet(PolyWorld world, SystemMeta systemMeta, T childT);

    private ISystemParam<T> _param;
    private OnInit? _init;
    private OnEval? _eval;
    private OnGet? _get;

    public HookedParam(ISystemParam<T> param, OnInit? init, OnEval? eval, OnGet? get)
    {
        _param = param;
        _init = init;
        _eval = eval;
        _param = param;
    }

    public void Initialize(PolyWorld world, SystemMeta meta)
    {
        _param.Initialize(world, meta);
        _init?.Invoke(world, meta);
    }

    public void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration)
    {
        _param.EvaluateNewTable(meta, table, tableGeneration);
        _eval?.Invoke(meta, table, tableGeneration);
    }

    public T Get(PolyWorld world, SystemMeta systemMeta)
    {
        var value = _param.Get(world, systemMeta);
        if (_get == null)
        {
            return value;
        }
        return _get.Invoke(world, systemMeta, value);
    }
}

public class WriteAnnotatedParam<T> : ISystemParam<T>
{
    private ISystemParam<T> _param;

    public WriteAnnotatedParam(ISystemParam<T> param, IEnumerable<ulong> writes)
    {
        Writes = writes;
        _param = param;
    }

    protected IEnumerable<ulong> Writes;

    public void Initialize(PolyWorld world, SystemMeta meta)
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

    public void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration)
    {
        foreach (var write in Writes)
        {
            if (table.Has(write))
            {
                meta.TableComponentAccess.AddWrite(new TableComponentId(tableGeneration, write));
            }
        }
    }

    public T Get(PolyWorld world, SystemMeta systemMeta) => throw new NotImplementedException();
}

public class QueryParam : ISystemParam<Query>
{
    public QueryParam(Query query) => _query = query;
    private readonly Query _query;

    public void Initialize(PolyWorld world, SystemMeta meta)
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
                    access.AddRead(term.Id());
                    break;

                case flecs.ecs_oper_kind_t.EcsOptional:
                    // Add to read but don't add a With/Required filter. 
                    access.Access.AddRead(term.Id());
                    break;
                default:
                    // I'm going to ignore AndFrom/prefab behavior for now. If this comes up i'll add it.
                    throw new InvalidOperationException($"Unknown query term oper: {term.Oper()}");
            }
        }
        meta.ComponentAccessSet.Add(access);
    }

    public void EvaluateNewTable(SystemMeta meta, Table table, int tableGen)
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

    public Query Get(PolyWorld world, SystemMeta systemMeta) => _query;
}

public class LocalParam<T> : ISystemParam<T>
{
    protected T Value;

    public void Initialize(PolyWorld world, SystemMeta meta)
    {
        Value = default;
    }

    public void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration) { }

    public T Get(PolyWorld world, SystemMeta systemMeta) => Value;
}

/// <summary>
/// A simple parameter that access a world singleton. <see cref="Res"/> and <see cref="ResMut{T}"/> should be used in actual applications. 
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonParam<T> : ISystemParam<Ref<T>>
{
    protected ulong _id;

    public void Initialize(PolyWorld world, SystemMeta meta)
    {
        unsafe
        {
            _id = Type<T>.Id(world.World);
            meta.ComponentAccessSet.AddUnfilteredWrite(_id);
        }
    }

    public void EvaluateNewTable(SystemMeta meta, Table table, int tableGen)
    {
        if (table.Has(_id))
        {
            meta.TableComponentAccess.AddWrite(new TableComponentId(tableGen, _id));
        }
    }

    public Ref<T> Get(PolyWorld world, SystemMeta systemMeta) => world.World.GetRef<T>();
}

public class BiParam<T1, T2> : ISystemParam<(T1, T2)>
{
    public BiParam(ISystemParam<T1> p1, ISystemParam<T2> p2) => _params = (p1, p2);
    private readonly (ISystemParam<T1>, ISystemParam<T2>) _params;

    public void Initialize(PolyWorld world, SystemMeta meta)
    {
        _params.Item1.Initialize(world, meta);
        _params.Item2.Initialize(world, meta);
    }

    public void EvaluateNewTable(SystemMeta meta, Table table, int tableGen)
    {
        _params.Item1.EvaluateNewTable(meta, table, tableGen);
        _params.Item2.EvaluateNewTable(meta, table, tableGen);
    }

    public (T1, T2) Get(PolyWorld world, SystemMeta systemMeta)
    {
        return (_params.Item1.Get(world, systemMeta), _params.Item2.Get(world, systemMeta));
    }
}

public class TriParam<T1, T2, T3> : ISystemParam<(T1, T2, T3)>
{
    public TriParam(ISystemParam<T1> p1, ISystemParam<T2> p2, ISystemParam<T3> p3) => _params = (p1, p2, p3);
    private readonly (ISystemParam<T1>, ISystemParam<T2>, ISystemParam<T3>) _params;

    public void Initialize(PolyWorld world, SystemMeta meta)
    {
        _params.Item1.Initialize(world, meta);
        _params.Item2.Initialize(world, meta);
        _params.Item3.Initialize(world, meta);
    }

    public void EvaluateNewTable(SystemMeta meta, Table table, int tableGen)
    {
        _params.Item1.EvaluateNewTable(meta, table, tableGen);
        _params.Item2.EvaluateNewTable(meta, table, tableGen);
        _params.Item3.EvaluateNewTable(meta, table, tableGen);
    }

    public (T1, T2, T3) Get(PolyWorld world, SystemMeta systemMeta)
    {
        return (_params.Item1.Get(world, systemMeta), _params.Item2.Get(world, systemMeta), _params.Item3.Get(world, systemMeta));
    }
}
