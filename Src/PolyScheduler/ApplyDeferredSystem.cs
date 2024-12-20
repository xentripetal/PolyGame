namespace PolyScheduler;

public class ApplyDeferredSystem<TContext> : ISystem<TContext> where TContext : IContext<TContext> {
    public bool Equals(ISystem<TContext>? other)
    {
        return ReferenceEquals(this, other);
    }

    public string Name => "ApplyDeferredSystem";
    public ISystemSet SetAlias => new ReferenceSystemSet(this);

    public bool HasDeferred => false;

    public bool IsExclusive => true;

    public void Initialize(TContext context)
    {
    }

    /// <summary>
    /// System doesn't actually do anything. By being an exclusive system, it will enforce the executor to flush deferred operations.
    /// </summary>
    /// <param name="context"></param>
    public void Run(TContext context)
    {
    }
}