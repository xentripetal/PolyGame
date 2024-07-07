using Flecs.NET.Core;
using PolyECS.Scheduling.Graph;

namespace PolyECS.Systems;

public abstract class ClassSystem<T> : RunSystem
{
    public ClassSystem(ISystemParam<T> param, string name)
    {
        Meta = new SystemMeta(name);
        Param = param;
        set = new SystemTypeSet(this);
    }

    protected SystemSet set;

    private PolyWorld? _world;
    protected SystemMeta Meta;
    protected ISystemParam<T> Param;
    protected int tableGeneration = 0;

    public override void Initialize(PolyWorld world)
    {
        if (_world != null)
        {
            if (world != _world)
            {
                throw new InvalidOperationException("System cannot be initialized with a different world than the one it was created with.");
            }
        }
        else
        {
            _world = world;
            Param.Initialize(world, Meta);
        }
    }

    public override object? Run(object? i, PolyWorld world)
    {
        var p = Param.Get(world, Meta);
        Run(p);
        return i;
    }

    public abstract void Run(T param);

    public override bool HasDeferred => Meta.HasDeferred;

    public override bool IsExclusive => false;

    public override Access<ulong> GetAccess()
    {
        return Meta.ComponentAccessSet.CombinedAccess;
    }

    public override Access<TableComponentId> GetTableAccess()
    {
        return Meta.TableComponentAccess;
    }

    public override List<SystemSet> GetDefaultSystemSets()
    {
        return new List<SystemSet>([set]);
    }

    public override void UpdateTableComponentAccess(TableCache cache)
    {
        (var oldGeneration, tableGeneration) = (tableGeneration, cache.Generation);
        for (int i = oldGeneration; i < tableGeneration; i++)
        {
            Param.EvaluateNewTable(Meta, cache[i], i);
        }
    }
}
