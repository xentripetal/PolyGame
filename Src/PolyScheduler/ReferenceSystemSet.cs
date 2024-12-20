using System.Diagnostics.CodeAnalysis;

namespace PolyScheduler;

/// <summary>
/// A system set that compares itself based on an underlying reference
/// Generally 
/// </summary>
[method: SetsRequiredMembers]
public class ReferenceSystemSet(object reference): ISystemSet
{
    public required object Reference = reference;
    public bool Equals(ISystemSet? other)
    {
        if (other is ReferenceSystemSet otherRef)
        {
            return ReferenceEquals(Reference, otherRef.Reference);
        }

        return false;
    }

    public string Name
    {
        get => $"ReferenceSet";
    }

    public bool IsSystemAlias
    {
        get => false;
    }
}