namespace PolyScheduler;

public interface ISystem<TContext> : IEquatable<ISystem<TContext>> {
    /// <summary>
    /// The name of the system for debugging purposes.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The set that represents the system.
    /// </summary>
    public ISystemSet SetAlias { get; }

    /// <summary>
    ///  Whether the system has deferred operations that need to be flushed to the context
    /// </summary>
    public bool HasDeferred { get; }

    /// <summary>
    /// Whether the system requires exclusive access to the context.
    /// </summary>
    public bool IsExclusive { get; }

    public void Initialize(TContext context);
    public void Run(TContext context);
}