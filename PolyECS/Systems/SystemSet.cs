using PolyECS.Scheduling.Graph;

namespace PolyECS.Systems;

public interface SystemSet : IEquatable<SystemSet>
{
    public string Name();
    public Type? SystemType();
}

public struct NamedSet : SystemSet
{
    public NamedSet(string name)
    {
        _name = name;
    }

    private string _name;

    public bool Equals(SystemSet? other)
    {
        if (other is NamedSet otherNamed)
        {
            return _name == otherNamed._name;
        }
        return false;
    }

    public string Name() => _name;

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


    public Type? SystemType() => null;

    public string Name() => $"AnonymousSet {Id}";
}

/// <summary>
/// A <see cref="SystemSet"/> grouping instances of the same <see cref="BaseSystem{TIn,TOut}"/>.
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
    public RunSystem System;

    public SystemTypeSet(RunSystem sys)
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

    public string Name() => $"SystemTypeSet {System.GetType().Name}";

    public Type? SystemType()
    {
        return System.GetType();
    }
}
