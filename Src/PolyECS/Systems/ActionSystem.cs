namespace PolyECS.Systems;

/// <summary>
/// A system with no input and no output. Needed as a stub to support <see cref="System.Action"/> with no parameters
/// </summary>
public class ActionSystem : RunnableSystem<Empty>
{
    public ActionSystem(Action action) : base(action.ToString())
    {
        Action = action;
        DefaultSets.Add(new SystemReferenceSet(this));
    }

    protected Action Action;

    protected override ISystemParam<Empty> CreateParam(PolyWorld world)
    {
        return new VoidParam();
    }

    public override Empty Run(Empty input, Empty param)
    {
        Action.Invoke();
        return input;
    }

    public static implicit operator ActionSystem(Action action)
    {
        return new ActionSystem(action);
    }
}

public class ActionSystem<T> : RunnableSystem<T> where T : IIntoSystemParam<T>
{
    public ActionSystem(Action<T> action) : base(action.ToString())
    {
        Action = action;
        DefaultSets.Add(new SystemReferenceSet(this));
    }

    protected Action<T> Action;

    protected override ISystemParam<T> CreateParam(PolyWorld world)
    {
        return T.IntoParam(world);
    }

    public override Empty Run(Empty input, T param)
    {
        Action.Invoke(param);
        return input;
    }

    public static implicit operator ActionSystem<T>(Action<T> action)
    {
        return new ActionSystem<T>(action);
    }
}

public static class LambdaExtension
{
    public static RunSystem IntoSystem(this Action a)
    {
        return new ActionSystem(a);
    }

    public static RunSystem IntoSystem<T>(this Action<T> a) where T : IIntoSystemParam<T>
    {
        return new ActionSystem<T>(a);
    }
}
