namespace PolyECS.Systems;

/// <summary>
///     A system with no input and no output. Needed as a stub to support <see cref="System.Action" /> with no parameters
/// </summary>
public class ActionSystem : RunnableSystem<Empty>
{
    protected Action Action;

    public ActionSystem(Action action) : base(action.ToString())
    {
        Action = action;
        DefaultSets.Add(new SystemReferenceSet(this));
    }

    protected override ISystemParam<Empty> CreateParam(PolyWorld world) => new VoidParam();

    public override Empty Run(Empty input, Empty param)
    {
        Action.Invoke();
        return input;
    }

    public static implicit operator ActionSystem(Action action) => new (action);
}

public class ActionSystem<T> : RunnableSystem<T> where T : IIntoSystemParam<T>
{
    protected Action<T> Action;

    public ActionSystem(Action<T> action) : base(action.ToString())
    {
        Action = action;
        DefaultSets.Add(new SystemReferenceSet(this));
    }

    protected override ISystemParam<T> CreateParam(PolyWorld world) => T.IntoParam(world);

    public override Empty Run(Empty input, T param)
    {
        Action.Invoke(param);
        return input;
    }

    public static implicit operator ActionSystem<T>(Action<T> action) => new (action);
}

public static class LambdaExtension
{
    public static RunSystem IntoSystem(this Action a) => new ActionSystem(a);

    public static RunSystem IntoSystem<T>(this Action<T> a) where T : IIntoSystemParam<T> => new ActionSystem<T>(a);
}
