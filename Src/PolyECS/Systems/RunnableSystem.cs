namespace PolyECS.Systems;

/// <summary>
/// A parameter based system that takes no input and returns no output. Runnable denotes that it is the standard system type that is ran by the scheduler <see cref="RunSystem"/>
/// </summary>
/// <typeparam name="T">Parameter type</typeparam>
public abstract class RunnableSystem<T> : ParameterSystem<T, object?, object?>
{
    protected RunnableSystem(string name) : base(name) { }
    
    protected RunnableSystem() : base() { }
}
