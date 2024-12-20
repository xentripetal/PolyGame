namespace PolyScheduler;

/// <summary>
/// A generated anonymous set that is used for constructing system graphs from entries.
/// Generally 
/// </summary>
public class AnonymousSystemSet : ISystemSet
{
    public bool Equals(ISystemSet? other)
    {
        if (other is AnonymousSystemSet otherAnonymous)
        {
            return ReferenceEquals(this, otherAnonymous);
        }

        return false;
    }

    public string Name
    {
        get => $"AnonymousSet)";
    }

    public bool IsSystemAlias
    {
        get => false;
    }
}