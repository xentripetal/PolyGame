namespace PolyECS.Systems;

public interface SystemSet : IEquatable<SystemSet>
{
    public bool IsAnonymous();
    public Type? SystemType();
}

public struct AnonymousSet : SystemSet
{
    public ulong Id;
    
    public AnonymousSet(ulong id)
    {
        Id = id;
    }

    public bool Equals(SystemSet? other)
    {
        if (other is AnonymousSet otherAnonymous)
        {
            return Id == otherAnonymous.Id;
        }
        return false;
    }

    public bool IsAnonymous()
    {
        return true;
    }

    public Type? SystemType() => null;
}

/// <summary>
/// A <see cref="SystemSet"/> grouping instances of the same <see cref="System"/>.
///
/// This kind of set is automatically populated and thus has some special rules:
/// <list type="bullet">
/// <item>You cannot manually add members</item>
/// <item>You cannot configure them</item>
/// <item>You cannot order something relative to one if it has more than one member</item>
/// </list>
/// </summary>
struct SystemTypeSet<TSystem, TComponent> : SystemSet where TSystem : System<TComponent>
{
    public bool Equals(SystemSet? other)
    {
        if (other is SystemTypeSet<TSystem, TComponent> otherType)
        {
            return true;
        }
        return false;
    }

    public bool IsAnonymous()
    {
        return false;
    }

    public Type? SystemType() => typeof(TSystem);
}
