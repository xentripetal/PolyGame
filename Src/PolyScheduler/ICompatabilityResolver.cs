namespace PolyScheduler;

/// <summary>
/// Reports the compatability between two different systems with the same context.
/// </summary>
public interface ICompatabilityResolver<TContext, TResource>
{
    public bool IsCompatible(ISystem<TContext> a, ISystem<TContext> b);
    public List<TResource> GetConflicts(ISystem<TContext> a, ISystem<TContext> b);
}