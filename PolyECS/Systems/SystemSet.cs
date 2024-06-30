namespace PolyECS.Systems;

public interface SystemSet : IEquatable<SystemSet>
{
    public bool IsAnonymous();
    public Type? SystemType();
}

public struct NamedSet : SystemSet
{
    public NamedSet(string name)
    {
        Name = name;
    }
    
    public string Name;

    public bool Equals(SystemSet? other)
    {
        if (other is NamedSet otherNamed)
        {
            return Name == otherNamed.Name;
        }
        return false;
    }

    public bool IsAnonymous() => false;

    public Type? SystemType() => null;
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
/// A <see cref="SystemSet"/> grouping instances of the same <see cref="ASystem"/>.
///
/// This kind of set is automatically populated and thus has some special rules:
/// <list type="bullet">
/// <item>You cannot manually add members</item>
/// <item>You cannot configure them</item>
/// <item>You cannot order something relative to one if it has more than one member</item>
/// </list>
/// </summary>
public class SystemTypeSet : SystemSet
{
    public ASystem System;
    public SystemTypeSet(ASystem sys)
    {
        System = sys;
    }
    public bool Equals(SystemSet? other)
    {
        if (other is SystemTypeSet otherType)
        {
            return System == otherType.System;
        }
        return false;
    }

    public bool IsAnonymous()
    {
        return false;
    }

    public Type? SystemType()
    {
        return System.GetType();
    }
}
