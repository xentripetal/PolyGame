using Friflo.Json.Fliox.Transform.Query.Ops;

namespace PolyFlecs.Systems;

public abstract class System
{
    public abstract void Initialize(World world);
    public abstract void Update();

    /// <summary>
    /// Returns `true` if the system has deferred buffers
    /// </summary>
    public virtual bool HasDeferred()
    {
        return false;
    }

    public virtual bool IsExclusive()
    {
        return false;
    }

    public abstract Access GetAccess();

    public abstract List<SystemSet> GetDefaultSystemSets();

    public SystemSet ToSystemSet()
    {
        return new SystemTypeSet<System>();
    }

    public void ApplyDeferred(World world)
    {
        throw new NotImplementedException();
    }
}
