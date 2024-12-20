namespace PolyScheduler;

public interface IContext<TSelf> {
    public void Initialize();
    public void RunSystem(ISystem<TSelf> system);
    public bool EvaluateCondition(ICondition<TSelf> condition);
}

/// <summary>
/// A schedule context that has a global defer state for committing changes. This is used for some ECS libraries like
/// flecs and fennecs. Others libraries use buffers for each system which do not require a global defer state and won't benefit from this.
/// </summary>
public interface IDeferrableContext<TSelf> : IContext<TSelf> {
    public void BeginDefer();
    public void EndDefer();
}