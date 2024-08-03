using Flecs.NET.Core;

namespace PolyECS.Systems;

/// <summary>
/// Tracks read and write access to specific elements in a collection.
///
/// Used internally to ensure soundness during system initialization and execution.
/// </summary>
/// <remarks>Port of bevy_ecs::query::access::Access. Does not used a <see cref="FixedBitSet"/> as I'm trying to avoid requiring a component registry</remarks>
/// 
public class Access<T> : IEquatable<Access<T>>
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
    /// This field is a performance optimization (also harder to mess up for soundness)
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
        return !Writes.Overlaps(other.ReadsAndWrites) && !other.Writes.Overlaps(ReadsAndWrites);
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

    /// <summary>
    /// Evaluates if other contains at least all the values in this
    /// </summary>
    /// <param name="other">Potential superset</param>
    /// <returns>True if this set is a subset of other</returns>
    public bool IsSubset(Access<T> other)
    {
        if (WritesAll)
        {
            return other.WritesAll;
        }
        if (other.WritesAll)
        {
            return true;
        }
        if (ReadsAll)
        {
            return other.ReadsAll;
        }
        if (other.ReadsAll)
        {
            return Writes.IsSubsetOf(other.Writes);
        }
        return ReadsAndWrites.IsSubsetOf(other.ReadsAndWrites) && Writes.IsSubsetOf(other.Writes);
    }

    public bool Equals(Access<T>? other)
    {
        return other != null && ReadsAndWrites.SetEquals(other.ReadsAndWrites) && Writes.SetEquals(other.Writes) && ReadsAll == other.ReadsAll && WritesAll == other.WritesAll &&
               Archetypal.SetEquals(other.Archetypal);
    }

    public override bool Equals(object? obj) => obj is Access<T> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(ReadsAndWrites, Writes, ReadsAll, WritesAll, Archetypal);
}

/// <summary>
/// A set of filters that describe table level filters for a query. 
/// </summary>
/// <remarks>Based on bevy_ecs::query::access::AccessFilters{T}</remarks>
public struct AccessFilters<T> : IEquatable<AccessFilters<T>>
{
    /// <remarks>
    /// Bevy can use a FixedBitSet here because its component ids are contiguous and can be used as indices. However, flecs supports arbitrary ids, so we use a hashset instead.
    /// </remarks>>
    public HashSet<T> With;
    public HashSet<T> Without;

    public AccessFilters()
    {
        With = new ();
        Without = new ();
    }

    public bool IsRuledOutBy(AccessFilters<T> other)
    {
        return With.Overlaps(other.Without) || Without.Overlaps(other.With);
    }

    public AccessFilters<T> Clone()
    {
        return new AccessFilters<T>
        {
            With = new HashSet<T>((IEnumerable<T>)With),
            Without = new HashSet<T>((IEnumerable<T>)Without)
        };
    }

    public bool Equals(AccessFilters<T> other) => With.SetEquals(other.With) && Without.SetEquals(other.Without);

    public override bool Equals(object? obj) => obj is AccessFilters<T> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(With, Without);
}

public struct FilteredAccess<T> : IEquatable<FilteredAccess<T>>
{
    public Access<T> Access;
    public HashSet<T> Required;
    /// <summary>
    /// An array of filter sets to express `With` or `Without` clauses in disjunctive normal form, for example: `Or{(With{A}, With{B})}`.
    /// Filters like `(With{A}, Or{(With{B}, Without{C})}` are expanded into `Or{((With{A}, With{B}), (With{A}, Without{C}))}`.
    /// </summary>
    public List<AccessFilters<T>> FilterSets;

    public FilteredAccess()
    {
        Access = new ();
        Required = new ();
        FilterSets = new ([new AccessFilters<T>()]);
    }

    public FilteredAccess<T> AddRead(T type)
    {
        Access.AddRead(type);
        Required.Add(type);
        AndWith(type);
        return this;
    }

    public FilteredAccess<T> AddWrite(T type)
    {
        Access.AddWrite(type);
        Required.Add(type);
        AndWith(type);
        return this;
    }

    public FilteredAccess<T> AddRequired(T type)
    {
        Required.Add(type);
        return this;
    }

    /// <summary>
    /// Adds a `With` filter: corresponds to a conjunction (AND) operation.
    /// Suppose we begin with `Or{(With{A}, With{B})}`, which is represented by an array of two `AccessFilter` instances.
    /// Adding `AND With{C}` via this method transforms it into the equivalent of  `Or{((With{A}, With{C}), (With{B}, With{C}))}`.
    /// </summary>
    /// <param name="type"></param>
    public FilteredAccess<T> AndWith(T type)
    {
        foreach (var filter in FilterSets)
        {
            filter.With.Add(type);
        }
        return this;
    }

    /// <summary>
    /// Adds a `Without` filter: corresponds to a conjunction (AND) operation.
    /// 
    /// Suppose we begin with `Or{(With{A}, With{B})}`, which is represented by an array of two `AccessFilter` instances.
    /// Adding `AND Without{C}` via this method transforms it into the equivalent of  `Or{((With{A}, Without{C}), (With{B}, Without{C}))}`.
    /// </summary>
    public FilteredAccess<T> AndWithout(T type)
    {
        foreach (var filter in FilterSets)
        {
            filter.Without.Add(type);
        }
        return this;
    }

    /// <summary>
    /// Appends an array of filters: corresponds to a disjunction (OR) operation.
    ///
    /// As the underlying array of filters represents a disjunction,
    /// where each element (`AccessFilters`) represents a conjunction,
    /// we can simply append to the array.
    /// </summary>
    /// <param name="other"></param>
    public FilteredAccess<T> AppendOr(FilteredAccess<T> other)
    {
        FilterSets.AddRange(other.FilterSets);
        return this;
    }

    /// <summary>
    /// Adds all of the accesses from other to this
    /// </summary>
    /// <param name="other"></param>
    public FilteredAccess<T> ExtendAccess(Access<T> other)
    {
        Access.Extend(other);
        return this;
    }

    /// <summary>
    /// Returns true if this and other can be active at the same time.
    /// </summary>
    public bool IsCompatible(FilteredAccess<T> other)
    {
        if (Access.IsCompatible(other.Access))
        {
            return true;
        }

        // If the access instances are incompatible, we want to check that whether filters can
        // guarantee that queries are disjoint.
        // Since the `FilterSets` array represents a Disjunctive Normal Form formula ("ORs of ANDs"),
        // we need to make sure that each filter set (ANDs) rule out every filter set from the `other` instance.
        //
        // For example, `Query<&mut C, Or<(With<A>, Without<B>)>>` is compatible `Query<&mut C, (With<B>, Without<A>)>`,
        // but `Query<&mut C, Or<(Without<A>, Without<B>)>>` isn't compatible with `Query<&mut C, Or<(With<A>, With<B>)>>`.
        return FilterSets.All(filter => other.FilterSets.All(otherFilter => filter.IsRuledOutBy(otherFilter)));
    }

    /// <param name="other">Comparison Access</param>
    /// <returns>A vector of elements that this and other cannot access at the same time</returns>
    public IEnumerable<T> GetConflicts(FilteredAccess<T> other)
    {
        if (!IsCompatible(other))
        {
            return Access.GetConflicts(other.Access);
        }
        return Enumerable.Empty<T>();
    }

    /// <summary>
    /// Adds all access and filters from `other`.
    ///
    /// Corresponds to a conjunction operation (AND) for filters.
    ///
    /// Extending `Or{(With{A}, Without{B})}` with `Or{(With{C}, Without{D})}` will result in
    /// `Or{((With{A}, With{C}), (With{A}, Without{D}), (Without{B}, With{C}), (Without{B}, Without{D}))}`. 
    /// </summary>
    /// <param name="other"></param>
    public FilteredAccess<T> Extend(FilteredAccess<T> other)
    {
        Access.Extend(other.Access);
        Required.UnionWith(other.Required);

        // We can aFilteredAccess<T> allocating a new array of bitsets if `other` contains just a single set of filters:
        // in this case we can short-circuit by performing an in-place union for each bitset.
        if (other.FilterSets.Count == 1)
        {
            foreach (var filter in FilterSets)
            {
                filter.With.UnionWith(other.FilterSets[0].With);
                filter.Without.UnionWith(other.FilterSets[0].Without);
            }
            return this;
        }

        var newFilterSets = new List<AccessFilters<T>>(FilterSets.Count + other.FilterSets.Count);
        foreach (var filter in FilterSets)
        {
            foreach (var otherFilter in other.FilterSets)
            {
                var newFilter = filter.Clone();
                newFilter.With.UnionWith(otherFilter.With);
                newFilter.Without.UnionWith(otherFilter.Without);
                newFilterSets.Add(newFilter);
            }
        }
        FilterSets = newFilterSets;
        return this;
    }

    /// <summary>
    /// Sets the underlying unfiltered access as having access to all indexed elements.
    /// </summary>
    public FilteredAccess<T> ReadAll()
    {
        Access.ReadAll();
        return this;
    }

    /// <summary>
    /// Sets the underlying unfiltered access as having mutable access to all indexed elements.
    /// </summary>
    public FilteredAccess<T> WriteAll()
    {
        Access.WriteAll();
        return this;
    }

    /// <summary>
    /// Evaluates if other contains at least all the values in this
    /// </summary>
    /// <param name="other">Potential superset</param>
    /// <returns>True if this set is a subset of other</returns>
    public bool IsSubset(FilteredAccess<T> other)
    {
        return Required.IsSubsetOf(other.Required) && Access.IsSubset(other.Access);
    }

    /// <returns>The elements that this access filters for</returns>
    public IEnumerable<T> WithFilters()
    {
        return FilterSets.SelectMany(x => x.With);
    }

    /// <returns>The elements that this access filters out</returns>
    public IEnumerable<T> WithoutFilters()
    {
        return FilterSets.SelectMany(x => x.Without);
    }

    public bool Equals(FilteredAccess<T> other)
        => Access.Equals(other.Access) && Required.SetEquals(other.Required) && FilterSets.SequenceEqual(other.FilterSets);

    public override bool Equals(object? obj) => obj is FilteredAccess<T> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Access, Required, FilterSets);
}

/// <summary>
/// A collection of <see cref="FilteredAccess{T}"/> instances.
///
/// Used internally to statically check if systems have conflicting access.
///
/// It stores multiple sets of accesses.
/// <list type="bullet">
/// <item>A combined set, which is the access of all filters in this set combined.</item>
/// <item>The set of access of each individual filters in this set </item>
/// </list>
/// </summary>
public struct FilteredAccessSet<T>
{
    public Access<T> CombinedAccess;
    public List<FilteredAccess<T>> FilteredAccesses;

    public FilteredAccessSet()
    {
        CombinedAccess = new ();
        FilteredAccesses = new ();
    }

    /// <summary>
    /// If the type is already read, this will upgrade it to a write for each subaccess that reads it
    /// </summary>
    /// <param name="type"></param>
    /// <returns>true if there was a read to turn into a write</returns>
    public bool UpgradeReadToWrite(T type)
    {
        if (!CombinedAccess.ReadsAll && CombinedAccess.HasRead(type))
        {
            return false;
        }
        foreach (var filter in FilteredAccesses)
        {
            // If the access includes a read of the write, we want to add the write to the access.
            if (filter.Access.ReadsAll || filter.Required.Contains(type))
            {
                filter.AddWrite(type);
            }
        }
        CombinedAccess.AddWrite(type);
        return true;
    }

    /// <summary>
    /// Access conflict resolution happen in two steps:
    /// <list type="number">
    /// <item> A "coarse" check, if there is no mutual unfiltered conflict between
    ///    `self` and `other`, we already know that the two access sets are
    ///    compatible.</item>
    /// <item> A "fine grained" check, it kicks in when the "coarse" check fails.
    ///    the two access sets might still be compatible if some of the accesses
    ///    are restricted with the [`With`](super::With) or [`Without`](super::Without) filters so that access is
    ///    mutually exclusive. The fine grained phase iterates over all filters in
    ///    the `self` set and compares it to all the filters in the `other` set,
    ///    making sure they are all mutually compatible.
    /// </item>
    /// </list>
    /// </summary>
    /// <param name="other">comparison access set</param>
    /// <returns>True if this and other can be active at the same time</returns>
    public bool IsCompatible(FilteredAccessSet<T> other)
    {
        if (CombinedAccess.IsCompatible(other.CombinedAccess))
        {
            return true;
        }
        foreach (var filtered in FilteredAccesses)
        {
            foreach (var otherFiltered in other.FilteredAccesses)
            {
                if (!filtered.IsCompatible(otherFiltered))
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Returns a vector of elements that this set and `other` cannot access at the same time.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public IEnumerable<T> GetConflicts(FilteredAccessSet<T> other)
    {
        var conflicts = new HashSet<T>();
        if (!CombinedAccess.IsCompatible(other.CombinedAccess))
        {
            foreach (var filtered in FilteredAccesses)
            {
                foreach (var otherFiltered in other.FilteredAccesses)
                {
                    conflicts.UnionWith(filtered.GetConflicts(otherFiltered));
                }
            }
        }
        return conflicts;
    }

    /// <summary>
    /// Returns a vector of elements that this set and `other` cannot access at the same time.
    /// </summary>
    public IEnumerable<T> GetConflictsSingle(FilteredAccess<T> filteredAccess)
    {
        var conflicts = new HashSet<T>();
        if (!CombinedAccess.IsCompatible(filteredAccess.Access))
        {
            foreach (var filtered in FilteredAccesses)
            {
                conflicts.UnionWith(filtered.GetConflicts(filteredAccess));
            }
        }
        return conflicts;
    }

    /// <summary>
    /// Adds the filtered access to the set.
    /// </summary>
    public FilteredAccessSet<T> Add(FilteredAccess<T> filteredAccess)
    {
        CombinedAccess.Extend(filteredAccess.Access);
        FilteredAccesses.Add(filteredAccess);
        return this;
    }

    /// <summary>
    /// Adds a read access without filters to the set
    /// </summary>
    public FilteredAccessSet<T> AddUnfilteredRead(T element)
    {
        var filter = new FilteredAccess<T>();
        filter.AddRead(element);
        return Add(filter);
    }

    /// <summary>
    /// Adds a write access without filters to the set
    /// </summary>
    /// <param name="element"></param>
    public FilteredAccessSet<T> AddUnfilteredWrite(T element)
    {
        var filter = new FilteredAccess<T>();
        filter.AddWrite(element);
        return Add(filter);
    }

    /// <summary>
    /// Adds all the access from the passed set to this
    /// </summary>
    public FilteredAccessSet<T> Extend(FilteredAccessSet<T> filteredAccessSet)
    {
        CombinedAccess.Extend(filteredAccessSet.CombinedAccess);
        FilteredAccesses.AddRange(filteredAccessSet.FilteredAccesses);
        return this;
    }

    /// <summary>
    /// Marks the set as reading all possible elements of type T
    /// </summary>
    public FilteredAccessSet<T> ReadAll()
    {
        CombinedAccess.ReadAll();
        return this;
    }

    /// <summary>
    /// Marks the set as writing all of T
    /// </summary>
    public FilteredAccessSet<T> WriteAll()
    {
        CombinedAccess.WriteAll();
        return this;
    }

    /// <summary>
    /// Removes all accesses stored in this set
    /// </summary>
    public FilteredAccessSet<T> Clear()
    {
        CombinedAccess.Clear();
        FilteredAccesses.Clear();
        return this;
    }
}
