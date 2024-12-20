namespace PolyScheduler;

public interface ISystemSet : IEquatable<ISystemSet> {
    /// <summary>
    /// Display name for the set for debugging purposes.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Whether the set is an alias for a system, meaning the set refers to a single system and that system refers back to it.
    /// </summary>
    public bool IsSystemAlias { get; }
}