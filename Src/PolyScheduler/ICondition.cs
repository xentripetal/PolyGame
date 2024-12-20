namespace PolyScheduler;

public interface ICondition<TContext> : IEquatable<ICondition<TContext>> {
    public void Initialize(TContext context);
    public bool Check(TContext context);
}