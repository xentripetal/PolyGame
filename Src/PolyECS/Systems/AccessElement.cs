namespace PolyECS.Systems;

/// <summary>
/// An identifier for a resource or component that a system accesses. Used with <see cref="Access{T}"/>. 
/// </summary>
public struct AccessElement : IEquatable<AccessElement>
{
    public ulong Id;
    public ResourceType Type;

    public static AccessElement OfComponent(ulong id) => new AccessElement { Id = id, Type = ResourceType.Component };
    public static AccessElement OfResource(int id) => new AccessElement { Id = (ulong)id, Type = ResourceType.Resource };

    public bool Equals(AccessElement other)
    {
        return Id == other.Id && Type == other.Type;
    }

    public override bool Equals(object? obj)
    {
        return obj is AccessElement other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, (int)Type);
    }

    /// <summary>
    /// Gets the display name for an element in the context of a world.
    /// </summary>
    /// <param name="world"></param>
    /// <returns></returns>
    public string Name(PolyWorld world)
    {
        if (Type == ResourceType.Component)
            return world.FlecsWorld.Component(Id).Name();
        if (world.Resources.TryGetEntry((int)Id, out var res))
        {
            return res.Value.Type.Name;
        }

        return $"Unknown({Id})";
    }
}

public enum ResourceType : byte
{
    /// <summary>
    /// A component that a system accesses in a query
    /// </summary>
    Component,
    /// <summary>
    /// A resource that a system accesses
    /// </summary>
    Resource,
}
