namespace PolyFlecs.Systems;

/// <summary>
/// A special marker system that applies deferred buffers.
/// </summary>
public sealed class ApplyDeferredSystem : System
{
    private Access access = new Access().WriteAll();

    public override void Initialize(World world) { }

    public override void Update() { }

    public override Access GetAccess()
    {
        return access;
    }

    public override List<SystemSet> GetDefaultSystemSets() => throw new NotImplementedException();
}
