using PolyECS.Scheduling.Configs;

namespace PolyECS.Systems;

public interface ISystemSet : IEquatable<ISystemSet>, IIntoSystemSet, IIntoSystemSetConfigs
{
    /// <summary>
    /// The name of the set.
    /// </summary>
    /// <returns></returns>
    public string GetName();
    /// <summary>
    /// Whether the set represents a single system. 
    /// </summary>
    /// <returns></returns>
    public bool IsSystemAlias();
}

public readonly record struct NamedSet(string Name) : ISystemSet
{
    public bool IsSystemAlias() => false;

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
    public NodeConfigs<ISystemSet> IntoConfigs() => new SystemSetConfig(this);

    public override int GetHashCode() => Name.GetHashCode();
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

    public override int GetHashCode() => Id.GetHashCode();

    public bool IsSystemAlias() => false;

    public string GetName() => $"AnonymousSet {Id}";
    public ISystemSet IntoSystemSet() => this;
    public NodeConfigs<ISystemSet> IntoConfigs() => new SystemSetConfig(this);
}

/// <summary>
///     A <see cref="ISystemSet" /> grouping instances of the same <see cref="ISystem" />.
///     This kind of set is automatically populated and thus has some special rules:
///     <list type="bullet">
///         <item>You cannot manually add members</item>
///         <item>You cannot configure them</item>
///         <item>You cannot order something relative to one if it has more than one member</item>
///     </list>
/// </summary>
public class SystemReferenceSet(ISystem sys) : ISystemSet
{
    public readonly ISystem System = sys;

    public bool Equals(ISystemSet? other)
    {
        if (other is SystemReferenceSet otherType)
        {
            return System == otherType.System;
        }
        return false;
    }

    public string GetName() => $"SystemReferenceSet {System.GetType().Name}";

    public bool IsSystemAlias() => true;

    public ISystemSet IntoSystemSet() => this;
    public NodeConfigs<ISystemSet> IntoConfigs() => new SystemSetConfig(this);

    public override int GetHashCode() => System.GetHashCode();
}

public class SystemTypeSet : ISystemSet
{
    public SystemTypeSet(Type type)
    {
        // check if type implements ISystem interface
        if (!typeof(ISystem).IsAssignableFrom(type))
            throw new ArgumentException("Type must implement ISystem interface", nameof(type));
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

    public bool IsSystemAlias() => true;
    public NodeConfigs<ISystemSet> IntoConfigs() => new SystemSetConfig(this);

    public override int GetHashCode() => Type.GetHashCode();
}

public class SystemTypeSet<T>() : SystemTypeSet(typeof(T))
    where T : ISystem
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
    public EnumSystemSet(T set) => Value = set;

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

    public bool IsSystemAlias() => false;
    public NodeConfigs<ISystemSet> IntoConfigs() => new SystemSetConfig(this);

    public override int GetHashCode() => HashCode.Combine(typeof(T), Value);
}

/// <summary>
///     A system set that is defined by its type. All instances of the same type are part of the same set.
/// </summary>
public abstract class StaticSystemSet : ISystemSet
{
    public bool Equals(ISystemSet? other) => other is StaticSystemSet && other.GetType() == GetType();

    public string GetName() => GetType().Name;

    public bool IsSystemAlias() => false;
    public ISystemSet IntoSystemSet() => this;
    public NodeConfigs<ISystemSet> IntoConfigs() => new SystemSetConfig(this);

    public override int GetHashCode() => GetType().GetHashCode();
}
