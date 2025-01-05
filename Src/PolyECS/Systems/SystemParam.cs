using DotNext;
using Flecs.NET.Bindings;
using Flecs.NET.Core;

namespace PolyECS.Systems;

public interface IIntoSystemParam
{
    public ISystemParam IntoParam(PolyWorld world);
}

public interface IStaticSystemParam<T>
{
    public static abstract T BuildParamValue(PolyWorld world);
    public static abstract ISystemParam GetParam(PolyWorld world, T value);
}

public interface ISystemParam : IIntoSystemParam
{
    public void Initialize(PolyWorld world, SystemMeta meta);
    public void EvaluateNewStorage(SystemMeta meta, Storage storage);

    /// <summary>
    /// Marks if the parameter is ready and can be passed to a system. If false the system will not run this tick
    /// </summary>
    /// <param name="world"></param>
    /// <param name="systemMeta"></param>
    /// <returns></returns>
    public bool IsReady(PolyWorld world, SystemMeta systemMeta);
}

public abstract class SystemParam<T> : ISystemParam
{
    public abstract void Initialize(PolyWorld world, SystemMeta meta);
    public abstract void EvaluateNewStorage(SystemMeta meta, Storage storage);
    public abstract bool IsReady(PolyWorld world, SystemMeta systemMeta);

    public ISystemParam IntoParam(PolyWorld world)
    {
        return this;
    }
}

public class VoidParam : SystemParam<Empty>
{
    public override void Initialize(PolyWorld world, SystemMeta meta)
    { }

    public override void EvaluateNewStorage(SystemMeta meta, Storage storage)
    { }

    public override bool IsReady(PolyWorld world, SystemMeta systemMeta) => true;
}

public static class Param
{
    public static QueryParam Of(Query query) => new QueryParam(query);
    public static PolyWorldParam Of(PolyWorld world) => new PolyWorldParam();
    public static WorldParam OfWorld(World world) => new WorldParam();
    public static PolyWorldParam OfWorld() => new PolyWorldParam();
    public static ResParam<T> OfRes<T>() => new ResParam<T>();
    public static ResMutParam<T> OfResMut<T>() => new ResMutParam<T>();
}

public class ResParam<T> : SystemParam<Res<T>>
{
    protected int ResourceId;
    protected bool Optional;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        var underlyingType = Nullable.GetUnderlyingType(typeof(T));
        if (underlyingType != null)
        {
            Optional = true;
            ResourceId = world.RegisterResource(underlyingType);
        }
        else
            ResourceId = world.RegisterResource<T>();
        meta.Access.AddUnfilteredRead(AccessElement.OfResource(ResourceId));
        meta.StorageAccess.AddRead(new Storage(world.Resources[ResourceId]!.Value).Key);
    }

    public override void EvaluateNewStorage(SystemMeta meta, Storage storage)
    { }

    public override bool IsReady(PolyWorld world, SystemMeta systemMeta) =>
        Optional || world.Resources[ResourceId].HasValue;
}

public class ResMutParam<T> : SystemParam<ResMut<T>>
{
    protected int ResourceId;
    protected bool Optional;


    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        var underlyingType = Nullable.GetUnderlyingType(typeof(T));
        if (underlyingType != null)
        {
            Optional = true;
            ResourceId = world.RegisterResource(underlyingType);
        }
        else
            ResourceId = world.RegisterResource<T>();

        meta.Access.AddUnfilteredWrite(AccessElement.OfResource(ResourceId));
        meta.StorageAccess.AddWrite(new Storage(world.Resources[ResourceId]!.Value).Key);
    }

    public override void EvaluateNewStorage(SystemMeta meta, Storage storage)
    { }

    public override bool IsReady(PolyWorld world, SystemMeta systemMeta) =>
        Optional || world.Resources[ResourceId].HasValue;
}

public class WorldParam : SystemParam<World>
{
    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        meta.Access.WriteAll();
        meta.StorageAccess.WriteAll();
        meta.HasDeferred = true;
    }

    public override void EvaluateNewStorage(SystemMeta meta, Storage storage)
    { }

    public override bool IsReady(PolyWorld world, SystemMeta systemMeta) => true;
}

public class PolyWorldParam : SystemParam<PolyWorld>
{
    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        meta.Access.WriteAll();
        meta.StorageAccess.WriteAll();
        meta.HasDeferred = true;
    }

    public override void EvaluateNewStorage(SystemMeta meta, Storage storage)
    { }

    public override bool IsReady(PolyWorld world, SystemMeta systemMeta) => true;
}

public class QueryParam : SystemParam<Query>
{
    private readonly Query _query;
    public QueryParam(Query query) => _query = query;

    public override void Initialize(PolyWorld world, SystemMeta meta)
    {
        if (!_query.World().Equals(world.FlecsWorld))
        {
            throw new InvalidOperationException(
                "Query was created with a different world than the one it is being used with.");
        }

        var access = new FilteredAccess<AccessElement>();
        for (var i = 0; i < _query.TermCount(); i++)
        {
            var term = _query.Term(i);
            switch (term.Oper())
            {
                case flecs.ecs_oper_kind_t.EcsNot:
                    access.AndWithout(AccessElement.OfComponent(term.Id()));
                    break;

                case flecs.ecs_oper_kind_t.EcsAnd:
                    access.AndWith(AccessElement.OfComponent(term.Id()));
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

        meta.Access.Add(access);
    }

    private void AddTermAccess(Term term, FilteredAccess<AccessElement> access)
    {
        switch (term.InOut())
        {
            case flecs.ecs_inout_kind_t.EcsIn:
                access.AddRead(AccessElement.OfComponent(term.Id()));
                break;

            case flecs.ecs_inout_kind_t.EcsInOut:
            case flecs.ecs_inout_kind_t.EcsInOutDefault:
                access.AddRead(AccessElement.OfComponent(term.Id()));
                access.AddWrite(AccessElement.OfComponent(term.Id()));
                break;

            case flecs.ecs_inout_kind_t.EcsOut:
                access.AddWrite(AccessElement.OfComponent(term.Id()));
                break;
        }
    }

    public override void EvaluateNewStorage(SystemMeta meta, Storage storage)
    {
        if (storage.Type != StorageType.Table)
        {
            return;
        }

        var table = storage.Table!.Value;
        var tableTypes = table.Type();

        var tableHasAccess = (AccessElement e) => e.Type == ResourceType.Component && table.Has(e.Id);

        foreach (var filter in meta.Access.FilteredAccesses)
        {
            if (!filter.WithFilters().All(tableHasAccess))
            {
                continue;
            }

            if (filter.WithoutFilters().Any(tableHasAccess))
            {
                continue;
            }

            if (filter.Access.WritesAll)
            {
                meta.StorageAccess.WriteAll();
                // This is maximally permissive, so we can skip the rest of the filters.
                return;
            }

            if (filter.Access.ReadsAll)
            {
                meta.StorageAccess.ReadAll();
            }

            foreach (var read in filter.Access.ReadsAndWrites)
            {
                if (tableHasAccess(read))
                {
                    meta.StorageAccess.AddRead(new StorageKey(storage.Id, table.TypeIndex(read.Id), StorageType.Table));
                }
            }

            foreach (var write in filter.Access.Writes)
            {
                if (tableHasAccess(write))
                {
                    meta.StorageAccess.AddWrite(
                        new StorageKey(storage.Id, table.TypeIndex(write.Id), StorageType.Table));
                }
            }
        }
    }

    public override bool IsReady(PolyWorld world, SystemMeta systemMeta) => true;

    public static implicit operator QueryParam(Query query) => new(query);
}