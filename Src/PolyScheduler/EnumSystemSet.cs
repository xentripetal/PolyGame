namespace PolyScheduler;

public class EnumSystemSet<T>(T enumValue) : ISystemSet
    where T : struct, Enum
{
    public T Value { get; } = enumValue;

    public bool Equals(ISystemSet? other)
    {
        if (other is EnumSystemSet<T> otherEnum)
        {
            return Value.Equals(otherEnum.Value);
        }

        return false;
    }

    public override int GetHashCode() => HashCode.Combine(typeof(T), Value);

    public string Name
    {
        get => $"{typeof(T).Name}({Enum.GetName(Value)})";
    }

    public bool IsSystemAlias
    {
        get => false;
    }
}