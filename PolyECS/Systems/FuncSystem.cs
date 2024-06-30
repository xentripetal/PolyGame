using Flecs.NET.Core;

namespace PolyECS.Systems;

public abstract class SimpleSystem : ASystem
{
    public SimpleSystem()
    {
        sets.Add(new SystemTypeSet(this));
    }

    protected Access<UntypedComponent> Access = new ();
    public override Access<UntypedComponent> GetAccess() => Access;

    protected List<SystemSet> sets = new ();

    public override List<SystemSet> GetDefaultSystemSets() => sets;

    public override void ApplyDeferred(World scheduleWorld)
    {
        scheduleWorld.DeferEnd();
        scheduleWorld.DeferBegin();
    }
}
