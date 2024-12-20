using Flecs.NET.Core;

namespace PolyECS;

public struct Storage
{
    public int Generation;
    public StorageType Type;
    public Table? Table;
    public ResourceEntry? Resource;
}

public enum StorageType : byte
{
    Table,
    Resource,
}
