using DotNext;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using PolyECS.Queries;

namespace PolyECS.Systems;

public interface IIntoSystemParam<T>
{
    public abstract static ITSystemParam<T> IntoParam(PolyWorld world);
}

public interface ITSystemParam<T> : ISystemParam
{
    public T Get(PolyWorld world, SystemMeta systemMeta);
}

public interface ISystemParam
{
    public void Initialize(PolyWorld world, SystemMeta meta);
    public void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration);

    /// <summary>
    /// Marks if the parameter is ready and can be passed to a system. If false the system will not run this tick
    /// </summary>
    /// <param name="world"></param>
    /// <param name="systemMeta"></param>
    /// <returns></returns>
    public bool IsGettable(PolyWorld world, SystemMeta systemMeta);

    public object Get(PolyWorld world, SystemMeta systemMeta);
}

public abstract class SystemParam<T> : ITSystemParam<T>
{
    public abstract void Initialize(PolyWorld world, SystemMeta meta);
    public abstract void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration);
    public abstract bool IsGettable(PolyWorld world, SystemMeta systemMeta);
    object ISystemParam.Get(PolyWorld world, SystemMeta systemMeta) => Get(world, systemMeta);

    public abstract T Get(PolyWorld world, SystemMeta systemMeta);
}

public class VoidParam : SystemParam<Empty>, IIntoSystemParam<Empty>
{
    public static ITSystemParam<Empty> IntoParam(PolyWorld world) => new VoidParam();

    public override void Initialize(PolyWorld world, SystemMeta meta) { }
    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGen) { }
    public override bool IsGettable(PolyWorld world, SystemMeta systemMeta) => true;

    public override Empty Get(PolyWorld world, SystemMeta systemMeta) => Empty.Instance;
}

public static class Param
{
    public static QueryParam Of(Query query) => new QueryParam(query);
    public static PolyWorldParam Of(PolyWorld world) => new PolyWorldParam();
    public static WorldParam OfWorld(World world) => new WorldParam();
    public static TQueryParam<T> Of<T>(TQuery<T> query) => new TQueryParam<T>(query);
    public static PolyWorldParam OfWorld() => new PolyWorldParam();
    public static ResParam<T> OfRes<T>() => new ResParam<T>();
    public static ResMutParam<T> OfResMut<T>() => new ResMutParam<T>();
}

public class ResParam<T> : SystemParam<Res<T>>
{
    private Entity _globalEntity;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        unsafe
        {
            world.RegisterResource<T>();
            _globalEntity = world.Entity<T>();
            meta.ComponentAccessSet.AddUnfilteredRead(Type<T>.Id(world.World.Handle));
        }
    }

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

    public override bool IsGettable(PolyWorld world, SystemMeta systemMeta) => world.GetResource<T>().HasValue;

    public override Res<T> Get(PolyWorld world, SystemMeta systemMeta) => world.GetResource<T>();
}

public class OptionalParam<TParam, TValue> : SystemParam<Optional<TValue>> where TParam : ITSystemParam<TValue>
{
    protected readonly TParam Param;
    public OptionalParam(TParam param) => Param = param;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        Param.Initialize(world, meta);
    }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration)
    {
        Param.EvaluateNewTable(meta, table, tableGeneration);
    }

    public override bool IsGettable(PolyWorld world, SystemMeta systemMeta) => true;

    public override Optional<TValue> Get(PolyWorld world, SystemMeta systemMeta)
    {
        if (Param.IsGettable(world, systemMeta))
        {
            return new Optional<TValue>(Param.Get(world, systemMeta));
        }
        return Optional<TValue>.None;
    }
}

public class ResMutParam<T> : SystemParam<ResMut<T>>
{
    private Entity _globalEntity;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        unsafe
        {
            world.RegisterResource<T>();
            _globalEntity = world.World.Entity<T>();
            meta.ComponentAccessSet.AddUnfilteredWrite(Type<T>.Id(world.World.Handle));
        }
    }

    public override bool IsGettable(PolyWorld world, SystemMeta systemMeta) => world.GetResourceMut<T>().HasValue;

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

public class WorldParam : SystemParam<World>
{
    private World _world;
    
    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        meta.ComponentAccessSet.WriteAll();
        meta.TableComponentAccess.WriteAll();
        meta.HasDeferred = true;
        _world = world.World;
    }
    
    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration) { }
    public override bool IsGettable(PolyWorld world, SystemMeta systemMeta) => true;
    public override World Get(PolyWorld world, SystemMeta systemMeta) => _world;
}

public class PolyWorldParam : SystemParam<PolyWorld>
{
    private PolyWorld _world;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        meta.ComponentAccessSet.WriteAll();
        meta.TableComponentAccess.WriteAll();
        meta.HasDeferred = true;
        _world = world;
    }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration) { }
    public override bool IsGettable(PolyWorld world, SystemMeta systemMeta) => true;

    public override PolyWorld Get(PolyWorld world, SystemMeta systemMeta) => _world;
}

public class TQueryParam<TData> : SystemParam<TQuery<TData>>
{
    protected QueryParam? Param;

    protected TQuery<TData>? Query;

    public TQueryParam()
    {
    }
    
    public TQueryParam(TQuery<TData> query)
    {
        Query = query;
    }

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        if (Query == null)
        {
            Query = new TQuery<TData>(world);
        }
        Param = new QueryParam(Query.Query);
        Param.Initialize(world, meta);
    }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration)
    {
        Param!.EvaluateNewTable(meta, table, tableGeneration);
    }

    public override bool IsGettable(PolyWorld world, SystemMeta systemMeta) => true;

    public override TQuery<TData> Get(PolyWorld world, SystemMeta systemMeta) => Query!;
}

public class TQueryParam<TData, TFilter> : SystemParam<TQuery<TData, TFilter>> where TFilter : IIntoFilter
{
    protected readonly QueryParam Param;

    protected TQuery<TData, TFilter> Query;

    public TQueryParam(TQuery<TData, TFilter> query)
    {
        Param = new QueryParam(query.Query);
        Query = query;
    }

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        Param.Initialize(world, meta);
    }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration)
    {
        Param.EvaluateNewTable(meta, table, tableGeneration);
    }

    public override bool IsGettable(PolyWorld world, SystemMeta systemMeta) => true;

    public override TQuery<TData, TFilter> Get(PolyWorld world, SystemMeta systemMeta) => Query;
}

public class QueryParam : SystemParam<Query>
{
    private readonly Query _query;
    public QueryParam(Query query) => _query = query;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        unsafe
        {
            if (_query.World() != world.World)
            {
                throw new InvalidOperationException("Query was created with a different world than the one it is being used with.");
            }
        }
        var access = new FilteredAccess<ulong>();
        for (var i = 0; i < _query.TermCount(); i++)
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

    public override bool IsGettable(PolyWorld world, SystemMeta systemMeta) => true;

    public override Query Get(PolyWorld world, SystemMeta systemMeta) => _query;

    public static implicit operator QueryParam(Query query) => new (query);
}

public class LocalParam<T> : SystemParam<T>
{
    protected T Value;

    public LocalParam() => Value = default;

    public LocalParam(T value) => Value = value;

    public override void Initialize(PolyWorld world, SystemMeta meta) { }

    public override void EvaluateNewTable(SystemMeta meta, Table table, int tableGeneration) { }
    public override bool IsGettable(PolyWorld world, SystemMeta systemMeta) => true;

    public override T Get(PolyWorld world, SystemMeta systemMeta) => Value;
}

/// <summary>
///     A simple parameter that access a world singleton. <see cref="Res" /> and <see cref="ResMut{T}" /> should be used in
///     actual applications.
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

    public override bool IsGettable(PolyWorld world, SystemMeta systemMeta) => true;

    public override Ref<T> Get(PolyWorld world, SystemMeta systemMeta) => world.World.GetRef<T>();
}
