using Flecs.NET.Bindings;
using Flecs.NET.Core;

namespace PolyECS.Systems;

public interface ISystemParamMetadata
{
    public void EvaluateNewStorage(SystemMeta meta, Storage storage);
    public bool IsReady(PolyWorld world);
}

public interface IIntoSystemParamMetadata
{
    public ISystemParamMetadata IntoParamMetadata(PolyWorld world, SystemMeta meta);
}

public class ResParamMetadata : ISystemParamMetadata
{
    protected ulong ResourceId;
    protected bool Optional;

    public ResParamMetadata(Type type, PolyWorld world, SystemMeta meta)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        var concreteType = underlyingType ?? type;
        Optional = underlyingType != null;
        ResourceId = world.RegisterResource(concreteType);
        meta.ResourceAccess.AddRead(ResourceId);
    }

    // No-op. Resources are seperate from tables
    public void EvaluateNewStorage(SystemMeta meta, Storage storage) { }

    public bool IsReady(PolyWorld world) => Optional || world.Resources.HasValue(ResourceId);
}

public class ResParamMetadata<T> : ResParamMetadata
{
    public ResParamMetadata(PolyWorld world, SystemMeta meta) : base(typeof(T), world, meta) { }
}

public class ResMutParamMetadata : ResParamMetadata
{
    public ResMutParamMetadata(Type type, PolyWorld world, SystemMeta meta) : base(type, world, meta)
    {
        meta.ResourceAccess.AddWrite(ResourceId);
    }
}

public class ResMutParamMetadata<T> : ResMutParamMetadata
{
    public ResMutParamMetadata(PolyWorld world, SystemMeta meta) : base(typeof(T), world, meta) { }
}

public class WorldParamMetadata : ISystemParamMetadata
{
    public WorldParamMetadata(SystemMeta meta)
    {
        meta.TableComponentAccess.WriteAll();
        meta.ComponentAccessSet.WriteAll();
        meta.ResourceAccess.WriteAll();
        meta.HasDeferred = true;
    }

    public void EvaluateNewStorage(SystemMeta meta, Storage storage) { }

    public bool IsReady(PolyWorld world) => true;
}


public class QueryParam : ISystemParamMetadata
{
    protected Query _query;
    
    public QueryParam(Query query, SystemMeta meta)
    {
        _query = query;
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

    public void EvaluateNewStorage(SystemMeta meta, Storage storage)
    {
        if (storage.Type != StorageType.Table)
        {
            return;
        }
        
        var table = storage.Table!.Value;
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
                    meta.TableComponentAccess.AddRead(new TableComponentId(storage.Generation, read));
                }
            }
            foreach (var write in filter.Access.Writes)
            {
                if (table.Has(write))
                {
                    meta.TableComponentAccess.AddWrite(new TableComponentId(storage.Generation, write));
                }
            }
        }
    }


    public bool IsReady(PolyWorld world) => true;
}
