using System.Runtime.CompilerServices;
using Flecs.NET.Core;

namespace PolyECS;

/// <summary>
///     A cache of all the tables in the world, indexed by component type. This is used to quickly find tables that match a
///     given set of component types.
/// </summary>
public class TableCache
{
    protected readonly Dictionary<Id, HashSet<Table>> exactTableLookup = new ();
    protected readonly HashSet<Table> tableSet = new ();
    protected Query tableQuery;
    protected List<Table> Tables = new ();
    protected World world;

    public TableCache(World world)
    {
        tableQuery = world.QueryBuilder().With(Ecs.Any).Build();
        this.world = world;
    }

    /// <summary>
    ///     The current generation of the cache, will increase each time a new table is added on <see cref="Update" />.
    ///     Used for tracking the addition of new tables by consumers of the cache.
    /// </summary>
    public int Generation { get; protected set; }

    public Table this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Tables[index];
    }

    public IEnumerable<Table> GetRange(int start, int count) => Tables.GetRange(start, count);

    public IEnumerable<Table> TablesForType(UntypedComponent component) => TablesForType(component.Id);

    public IEnumerable<Table> TablesForType(Id type)
    {
        if (exactTableLookup.TryGetValue(type, out var tables))
        {
            return tables;
        }
        return Enumerable.Empty<Table>();
    }

    public void Update()
    {
        // TODO - if we do a manual iter we can skip locking tables
        tableQuery.Iter(it => {
            RegisterTable(it.Table());
        });
    }

    public bool RegisterTable(Table table)
    {
        if (tableSet.Contains(table))
        {
            return false;
        }
        tableSet.Add(table);
        Tables.Add(table);
        Generation++;
        var types = table.Type();

        unsafe
        {
            foreach (var type in types)
            {
                // For some reason the Types array doesn't have a world handle, so we need to create a new Id with it
                var worldType = new Id(world.Handle, type.Value);
                AddTableToTypeLookup(worldType, table);

                // TODO - Do we need to account for inheritance here?
                if (worldType.IsPair())
                {
                    var first = worldType.First();
                    var second = worldType.Second();
                    AddTableToTypeLookup(new Id(world, first, Ecs.Wildcard), table);
                    AddTableToTypeLookup(new Id(world, Ecs.Wildcard, second), table);
                }
            }
        }
        return true;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AddTableToTypeLookup(Id type, Table table)
    {
        if (!exactTableLookup.ContainsKey(type))
        {
            exactTableLookup[type] = new HashSet<Table>();
        }
        exactTableLookup[type].Add(table);
    }
}
