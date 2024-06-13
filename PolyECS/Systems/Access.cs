namespace PolyECS.Systems;

/// <summary>
/// Tracks read and write access to specific elements in a collection.
///
/// Used internally to ensure soundness during system initialization and execution.
/// </summary>
/// <remarks>Port of bevy_ecs::query::access::Access. Does not used a <see cref="FixedBitSet"/> as I'm trying to avoid requiring a component registry</remarks>
/// 
public struct Access<T>
{
    /// <summary>
    /// All accessed elements
    /// </summary>
    public HashSet<T> ReadsAndWrites;
    /// <summary>
    /// Exclusively accessed elements
    /// </summary>
    public HashSet<T> Writes;
    /// <summary>
    /// Is true if this has access to all elements in the collection.
    /// This field is a performance optimization for <see cref="IScheduleWorld"/> (also harder to mess up for soundness)
    /// </summary>
    public bool ReadsAll;
    /// <summary>
    /// Is true if this has mutable access to all elements in the collection.
    /// If this is true, then <see cref="ReadsAll"/> must also be true.
    /// </summary>
    public bool WritesAll;
    /// <summary>
    /// Elements that are not accessed, but whose presence in an archetype affect query results
    /// </summary>
    public HashSet<T> Archetypal;

    /// <summary>
    /// Creates an empty [`Access`] collection.
    /// </summary>
    public Access()
    {
        ReadsAndWrites = new HashSet<T>();
        Writes = new HashSet<T>();
        Archetypal = new ();
    }

    /// <summary>
    /// Adds access to the given type.
    /// </summary>
    /// <param name="type"></param>
    public Access<T> AddRead(T type)
    {
        ReadsAndWrites.Add(type);
        return this;
    }

    /// <summary>
    /// Adds exclusive access to the given type.
    /// </summary>
    /// <param name="type"></param>
    public Access<T> AddWrite(T type)
    {
        ReadsAndWrites.Add(type);
        Writes.Add(type);
        return this;
    }

    /// <summary>
    /// Adds an archetypal (indirect) access to the element given by `index`.
    ///
    /// This is for elements whose values are not accessed (and thus will never cause conflicts),
    /// but whose presence in an archetype may affect query results.
    /// </summary>
    /// <param name="type"></param>
    public Access<T> AddArchetypal(T type)
    {
        Archetypal.Add(type);
        return this;
    }

    /// <summary>
    /// Returns `true` if this has read access to the given type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool HasRead(T type)
    {
        return ReadsAll || ReadsAndWrites.Contains(type);
    }

    /// <summary>
    /// Returns `true` if this can access anything
    /// </summary>
    /// <returns></returns>
    public bool HasAnyRead()
    {
        return ReadsAll || ReadsAndWrites.Count > 0;
    }

    /// <summary>
    /// Returns `true` if this can exclusively access the given type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool HasWrite(T type)
    {
        return WritesAll || Writes.Contains(type);
    }

    /// <summary>
    /// Returns true if this can access anything exclusively
    /// </summary>
    /// <returns></returns>
    public bool HasAnyWrite()
    {
        return WritesAll || Writes.Count > 0;
    }

    /// <summary>
    /// Returns true if this has an archetypal (indirect) access to the element given by `index`.
    ///
    /// This is an element whose value is not accessed (and thus will never cause conflicts),
    /// but whose presence in an archetype may affect query results.
    ///
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool HasArchetypal(T type)
    {
        return Archetypal.Contains(type);
    }

    /// <summary>
    /// Sets this as having access to all types.
    /// </summary>
    public Access<T> ReadAll()
    {
        ReadsAll = true;
        return this;
    }

    /// <summary>
    /// Sets this as having exclusive access to all types.
    /// </summary>
    public Access<T> WriteAll()
    {
        WritesAll = true;
        ReadsAll = true;
        return this;
    }

    /// <summary>
    /// Remove all writes
    /// </summary>
    /// <returns></returns>
    public Access<T> ClearWrites()
    {
        WritesAll = false;
        Writes.Clear();
        return this;
    }

    /// <summary>
    /// Removes all accesses
    /// </summary>
    /// <returns></returns>
    public Access<T> Clear()
    {
        ReadsAll = false;
        WritesAll = false;
        ReadsAndWrites.Clear();
        Writes.Clear();
        Archetypal.Clear();
        return this;
    }

    /// <summary>
    /// Adds all accesses from `other` to this.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public Access<T> Extend(Access<T> other)
    {
        ReadsAndWrites.UnionWith(other.ReadsAndWrites);
        Writes.UnionWith(other.Writes);
        Archetypal.UnionWith(other.Archetypal);
        ReadsAll = ReadsAll || other.ReadsAll;
        WritesAll = WritesAll || other.WritesAll;
        return this;
    }

    /// <summary>
    /// Returns `true` if the access and `other` can be active at the same time.
    ///
    /// <see cref="Access{T}"/> instances are incompatible if one can write
    /// an element that the other can read or write.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsCompatible(Access<T> other)
    {
        if (WritesAll)
        {
            return !other.HasAnyRead();
        }
        if (other.WritesAll)
        {
            return !HasAnyRead();
        }
        if (ReadsAll)
        {
            return !other.HasAnyWrite();
        }
        if (other.ReadsAll)
        {
            return !HasAnyWrite();
        }
        return !Writes.Overlaps(other.ReadsAndWrites) && !ReadsAndWrites.Overlaps(other.Writes);
    }

    /// <summary>
    /// Returns a vector of elements that the access and `other` cannot access at the same time.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public T[] GetConflicts(Access<T> other)
    {
        var conflicts = new HashSet<T>();
        if (ReadsAll)
        {
            // QUESTION: How to handle `other.writes_all`?
            conflicts.UnionWith(other.Writes);
        }
        if (other.ReadsAll)
        {
            conflicts.UnionWith(Writes);
        }
        if (WritesAll)
        {
            conflicts.UnionWith(other.ReadsAndWrites);
        }
        if (other.WritesAll)
        {
            conflicts.UnionWith(ReadsAndWrites);
        }

        conflicts.UnionWith(Writes.Intersect(other.ReadsAndWrites));
        conflicts.UnionWith(ReadsAndWrites.Intersect(other.Writes));
        return conflicts.ToArray();
    }

    public IEnumerable<T> GetReadsAndWrites()
    {
        return ReadsAndWrites;
    }

    public IEnumerable<T> GetWrites()
    {
        return Writes;
    }

    public IEnumerable<T> GetArchetypal()
    {
        return Archetypal;
    }
}
