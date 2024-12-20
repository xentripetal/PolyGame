using System.Diagnostics.CodeAnalysis;

namespace PolyScheduler;

/// <summary>
/// A system set that compares itself based on an underlying reference
/// Generally 
/// </summary>
[method: SetsRequiredMembers]
public class TypeSystemSet(Type type) : ISystemSet
{
    public required Type Type = type;

    public bool Equals(ISystemSet? other)
    {
        if (other is TypeSystemSet otherRef)
        {
            return Type == otherRef.Type;
        }

        return false;
    }

    public string Name
    {
        get => $"SystemSet({Type.Name})";
    }

    public bool IsSystemAlias
    {
        get => true;
    }
}

public class TypeSystemSet<T> : TypeSystemSet
{
    [SetsRequiredMembers]
    public TypeSystemSet() : base(typeof(T))
    {
    }
}