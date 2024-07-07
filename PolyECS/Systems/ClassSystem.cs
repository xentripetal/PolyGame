using Flecs.NET.Core;
using PolyECS.Scheduling.Graph;

namespace PolyECS.Systems;

public abstract class ClassSystem : RunSystem
{
    public ClassSystem(ISystemParam[] parameters, string name)
    {
        Meta = new SystemMeta(name);
        Parameters = parameters;
        set = new SystemTypeSet(this);
    }

    protected SystemSet set;

    private World? _world;
    protected SystemMeta Meta;
    protected ISystemParam[] Parameters;
    protected int tableGeneration;

    public override void Initialize(World world)
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
            foreach (var param in Parameters)
            {
                param.Initialize(world);
            }
        }
    }

    public override object? Run(object? i, World world)
    {
        Run(world);
        return i;
    }

    public abstract void Run(World world);

    public override bool HasDeferred => Meta.HasDeferred;

    public bool IsExclusive => false;

    public override Access<UntypedComponent> GetAccess()
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
        var oldGeneration = tableGeneration;
        tableGeneration = cache.Generation;

        foreach (var newTable in cache.GetRange(oldGeneration, tableGeneration - oldGeneration))
        {
            foreach (var param in Parameters)
            {
                param.EvaluateNewTable(Meta, newTable);
            }
        }
    }
}
