using PolyECS.Scheduling.Configs;

namespace PolyECS.Systems;

public interface ISystemSet : IEquatable<ISystemSet>, IIntoSystemSet
{
    public string GetName();
    public Type? SystemType();
}

public readonly record struct NamedSet(string Name) : ISystemSet
{
    public Type? SystemType() => null;

    public bool Equals(ISystemSet? other)
    {
        if (other is NamedSet otherNamed)
        {
            return Name == otherNamed.Name;
        }
        return false;
    }

    public string GetName() => Name;
    public ISystemSet IntoSystemSet() => this;
}

public readonly struct AnonymousSet(ulong id) : ISystemSet
{
    public readonly ulong Id = id;

    public bool Equals(ISystemSet? other)
    {
        if (other is AnonymousSet otherAnonymous)
        {
            return Id == otherAnonymous.Id;
        }
        return false;
    }


    public Type? SystemType() => null;

    public string GetName() => $"AnonymousSet {Id}";
    public ISystemSet IntoSystemSet() => this;
}

/// <summary>
/// A <see cref="ISystemSet"/> grouping instances of the same <see cref="BaseSystem{TIn,TOut}"/>.
///
/// This kind of set is automatically populated and thus has some special rules:
/// <list type="bullet">
/// <item>You cannot manually add members</item>
/// <item>You cannot configure them</item>
/// <item>You cannot order something relative to one if it has more than one member</item>
/// </list>
/// </summary>
public class SystemReferenceSet(RunSystem sys) : ISystemSet
{
    public readonly RunSystem System = sys;

    public bool Equals(ISystemSet? other)
    {
        if (other is SystemReferenceSet otherType)
        {
            return System == otherType.System;
        }
        return false;
    }

    public string GetName() => $"SystemReferenceSet {System.GetType().Name}";

    public Type SystemType()
    {
        return System.GetType();
    }

    public ISystemSet IntoSystemSet() => this;
}

public class SystemTypeSet : ISystemSet
{
    public SystemTypeSet(Type type)
    {
        if (!typeof(RunSystem).IsAssignableFrom(type))
        {
            throw new ArgumentException("Type must be a subclass of RunSystem to be a SystemTypeSet");
        }
        Type = type;
    }

    public Type Type { get; }

    public bool Equals(ISystemSet? other)
    {
        if (other is SystemTypeSet otherType)
        {
            return Type == otherType.Type;
        }
        return false;
    }

    public ISystemSet IntoSystemSet() => this;

    public string GetName() => $"SystemTypeSet {Type.Name}";

    public Type? SystemType() => Type;
}

public class SystemTypeSet<T>() : SystemTypeSet(typeof(T))
    where T : RunSystem
{
    public new bool Equals(ISystemSet? other)
    {
        if (other is SystemTypeSet<T> otherType)
        {
            return true;
        }
        return base.Equals(other);
    }

    public new string GetName() => $"SystemTypeSet {typeof(T).Name}";

    public new ISystemSet IntoSystemSet() => this;
}

public class EnumSystemSet<T> : ISystemSet where T : struct, Enum
{
    public EnumSystemSet(T set)
    {
        Value = set;
    }

    public T Value { get; }

    public bool Equals(ISystemSet? other)
    {
        if (other is EnumSystemSet<T> otherEnum)
        {
            return Value.Equals(otherEnum.Value);
        }
        return false;
    }

    public ISystemSet IntoSystemSet() => this;

    public string GetName() => $"{typeof(T).Name}({Enum.GetName(Value)})";

    public Type? SystemType() => null;
}

/// <summary>
/// A system set that is defined by its type. All instances of the same type are part of the same set.
/// </summary>
public abstract class StaticSystemSet : ISystemSet
{
    public bool Equals(ISystemSet? other)
    {
        return other is StaticSystemSet && other.GetType() == GetType();
    }

    public string GetName()
    {
        return GetType().Name;
    }

    public Type? SystemType() => null;
    public ISystemSet IntoSystemSet() => this;
}
