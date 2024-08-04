namespace PolyECS.Systems;

/// <summary>
/// A label for a schedule. Just a wrapper around a string for type enforcement.
/// </summary>
public readonly record struct ScheduleLabel(string Name)
{
    public bool Equals(ScheduleLabel other) => Name == other.Name;

    public readonly override int GetHashCode() => Name.GetHashCode();

    public override string ToString() => $"ScheduleLabel({Name})";
}
