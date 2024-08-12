using Flecs.NET.Core;

namespace PolyECS;

public struct TableComponentId(int tableGeneration, ulong id) : IEquatable<TableComponentId>
{
    public readonly int TableGeneration = tableGeneration;
    public readonly ulong Id = id;

    public Id ToId(World world)
    {
        return new Id
        {
            World = world,
            Value = Id
        };
    }

    public Id ToId()
    {
        return new Id
        {
            Value = Id
        };
    }

    public Table ToTable(TableCache cache)
    {
        return cache[TableGeneration];
    }

    public bool Equals(TableComponentId other) => TableGeneration == other.TableGeneration && Id == other.Id;

    public override bool Equals(object? obj) => obj is TableComponentId other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(TableGeneration, Id);
}
