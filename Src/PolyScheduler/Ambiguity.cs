namespace PolyScheduler;

public abstract record Ambiguity {
    /// <summary>
    ///     Default ambiguity handling: check for conflicts
    /// </summary>
    public record Check : Ambiguity { }

    /// <summary>
    ///     Ignore warnings with systems in any of these system sets. May contain duplicates.
    /// </summary>
    public record IgnoreWithSet(List<ISystemSet> Sets) : Ambiguity;

    /// <summary>
    ///     Ignore all warnings.
    /// </summary>
    public record IgnoreAll : Ambiguity { }
}