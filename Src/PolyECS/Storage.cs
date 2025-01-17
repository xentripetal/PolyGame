using Flecs.NET.Core;

namespace PolyECS;

public struct StorageKey : IEquatable<StorageKey>
{
    public StorageKey(int id, int subId, StorageType type)
    {
        Id = id;
        SubId = subId;
        Type = type;
    }

    public int Id;
    public int SubId;
    public StorageType Type;

    public Storage ToStorage(PolyWorld world)
    {
        return new Storage(world, Type, Id);
    }

    public bool Equals(StorageKey other)
    {
        return Id == other.Id && SubId == other.SubId && Type == other.Type;
    }

    public override bool Equals(object? obj)
    {
        return obj is StorageKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, SubId, (int)Type);
    }
}

/// <summary>
/// Reference to a resource or component in a table. Used for determining multithreaded access of systems.
/// </summary>
public struct Storage
{
    public Storage(int id, int subId, Table table)
    {
        Id = id;
        SubId = subId;
        Type = StorageType.Table;
        Table = table;
        table.Type().Get(subId);
    }

    public Storage(ResourceEntry resource)
    {
        Id = resource.Id;
        Type = StorageType.Resource;
        Resource = resource;
    }

    public Storage(PolyWorld world, StorageType type, int id, int subId = 0)
    {
        Id = id;
        Type = type;
        SubId = subId;
        if (type == StorageType.Table)
        {
            Table = world.TableCache[id];
        }
        else
        {
            world.Resources.TryGetEntry(id, out Resource);
        }
    }

    public int Id;
    public int SubId;
    public StorageType Type;
    public Table? Table;
    public UntypedComponent? Component
    {
        get
        {
            if (Type == StorageType.Table)
            {
                var id = Table?.Type().Get(SubId);
                if (id == null)
                {
                    return null;
                }
                unsafe
                {
                    return new UntypedComponent(id.Value.World, id.Value);
                }
            }
            return null;
        }
    }

    public ResourceEntry? Resource;

    public StorageKey Key => new StorageKey(Id, SubId, Type);
}

public enum StorageType : byte
{
    Table,
    Resource,
}