using System.Collections;

namespace PolyECS;

public class ResourceStorage : IEnumerable<ResourceEntry>
{
    protected Dictionary<Type, int> TypeLookup = new();
    protected List<ResourceEntry> Resources = new();

    public int Register<T>()
    {
        return Register(typeof(T), null);
    }

    public int Count => Resources.Count;

    public int Generation => Count;

    public int Register<T>(T value)
    {
        return Register(typeof(T), value);
    }

    public int RegisterDyn(Type type)
    {
        return Register(type, null);
    }

    public void Set<T>(T? value)
    {
        var isResource = TypeLookup.TryGetValue(typeof(T), out var id);
        if (!isResource)
        {
            Register(value);
        }
        else
        {
            Set(id, value);
        }
    }

    public void Set<T>(int id, T? value)
    {
        var index = (int)id;
        if (index >= Resources.Count)
        {
            throw new IndexOutOfRangeException();
        }

        Resources[index] = new ResourceEntry
        {
            Type = typeof(T),
            Resource = value,
            HasValue = value != null,
            Id = id
        };
    }

    public bool TryGet<T>(out T? value)
    {
        var isResource = TypeLookup.TryGetValue(typeof(T), out var id);
        if (!isResource)
        {
            value = default;
            return false;
        }

        return TryGet<T>(id, out value);
    }

    public T? Get<T>()
    {
        if (TryGet<T>(out T? value))
        {
            return value;
        }

        return default;
    }

    public bool TryGet<T>(int id, out T? value)
    {
        var hasValue = TryGet(id, out object? v);
        if (hasValue)
        {
            value = (T)v!;
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGet(int id, out object? value)
    {
        var index = (int)id;
        if (index >= Resources.Count)
        {
            value = default;
            return false;
        }

        var resource = Resources[index];
        if (resource.HasValue)
        {
            value = resource.Resource;
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetEntry(int id, out ResourceEntry? entry)
    {
        var index = (int)id;
        if (index >= Resources.Count)
        {
            entry = null;
            return false;
        }

        entry = Resources[index];
        return true;
    }

    public int Register(Type type, object? value = null)
    {
        if (TypeLookup.TryGetValue(type, out var id))
        {
            return id;
        }

        lock (Resources)
        {
            id = Resources.Count;
            TypeLookup[type] = id;
            Resources.Add(new ResourceEntry
            {
                Type = type,
                Resource = value,
                HasValue = value != null,
                Id = id
            });
        }

        return id;
    }

    public IEnumerator<ResourceEntry> GetEnumerator()
    {
        // Get a snapshot of the current resources. We don't want to lock the entire iteration and resources can never be removed so syncing the count is enough.
        int count = Resources.Count;

        for (int i = 0; i < count; i++)
        {
            yield return Resources[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool HasValue<T>()
    {
        return HasValue(typeof(T));
    }

    public bool HasValue(Type type)
    {
        if (!TypeLookup.TryGetValue(type, out var id))
        {
            return false;
        }

        return HasValue(id);
    }

    public bool HasValue(int resourceId)
    {
        var index = (int)resourceId;
        if (index >= Resources.Count)
        {
            return false;
        }

        return Resources[index].HasValue;
    }

    public ResourceEntry? this[int i]
    {
        get
        {
            if (i < 0 || i >= Resources.Count)
            {
                return null;
            }

            return Resources[i];
        }
    }
}

public struct ResourceEntry
{
    public Type Type;
    public object? Resource;
    public bool HasValue;
    public int Id;
}