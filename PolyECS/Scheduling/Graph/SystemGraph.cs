using System.Diagnostics;
using PolyECS.Systems.Configs;
using PolyECS.Systems.Graph;
using PolyECS.Systems;
using QuikGraph;
using QuikGraph.Algorithms;
using TinyEcs;

namespace PolyECS.Scheduling.Graph;

/// <summary>
/// Metadata for a Schedule.
///
/// The order isn't optimized; calling <see cref="BuildSchedule"/> return a SystemSchedule where the order is optimized for execution
/// </summary>
/// <remarks>
/// Port of bevy_ecs::schedule::SystemGraph
/// </remarks>
public class SystemGraph
{
    /// List of systems in the schedule
    protected List<ASystem> Systems = new ();
    /// List of conditions for each system, in the same order as `systems`
    protected List<List<Condition>> SystemConditions = new ();
    /// List of system sets in the schedule
    protected List<SystemSet> SystemSets = new ();

    /// List of conditions for each system set, in the same order as `system_sets`
    protected List<List<Condition>> SystemSetConditions = new ();

    /// Map from system set to node id
    public Dictionary<SystemSet, NodeId> SystemSetIds = new ();

    /// Systems that have not been initialized yet; for system sets, we store the index of the first uninitialized condition
    /// (all the conditions after that index still need to be initialized)
    protected List<(NodeId, int)> Uninit = new ();

    /// Directed acyclic graph of the hierarchy (which systems/sets are children of which sets)
    protected BidirectionalGraph<NodeId, Edge<NodeId>> Hierarchy = new ();

    /// Directed acyclic graph of the dependency (which systems/sets have to run before which other systems/sets)
    protected BidirectionalGraph<NodeId, Edge<NodeId>> Dependency = new ();

    public UndirectedGraph<NodeId, Edge<NodeId>> AmbiguousWith = new ();
    protected HashSet<NodeId> AmbiguousWithAll = new ();
    protected List<(NodeId, NodeId, ComponentInfo[])> ConflictingSystems = new ();
    protected ulong AnonymousSets;
    public bool Changed;
    public ScheduleBuildSettings Config = new ();

    /// Dependency edges that will **not** automatically insert an instance of `apply_deferred` on the edge.
    protected HashSet<(NodeId, NodeId)> NoSyncEdges = new ();
    protected Dictionary<uint, NodeId> AutoSyncNodeIds = new ();

    /// <summary>
    /// Returns the set at the given <see cref="NodeId"/>, if it exists
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public SystemSet? GetSetAt(NodeId id)
    {
        if (id.Type != NodeType.Set)
        {
            return null;
        }
        if (id.Id >= SystemSets.Count)
        {
            return null;
        }
        return SystemSets[id.Id];
    }

    /// <summary>
    /// Returns the <see cref="ASystem"/> at the given <see cref="NodeId"/>, if it exists
    /// </summary>
    /// <param name="id"></param>
    /// <returns>System for the given NodeId</returns>
    public ASystem? GetSystemAt(NodeId id)
    {
        if (id.Type != NodeType.System)
        {
            return null;
        }
        if (id.Id >= Systems.Count)
        {
            return null;
        }
        return Systems[id.Id];
    }



    /// <summary>
    /// Provides an iterator over all <see cref="ASystem"/>s in this schedule, along with their <see cref="Condition"/>s
    /// </summary>
    /// <returns></returns>
    public IEnumerable<(NodeId, ASystem, Condition[])> GetSystems()
    {
        for (var i = 0; i < Systems.Count; i++)
        {
            var system = Systems[i];
            var conditions = SystemConditions[i].ToArray();
            yield return (new NodeId(i, NodeType.System), system, conditions);
        }
    }

    public IEnumerable<(NodeId, SystemSet, Condition[])> GetSets()
    {
        for (var i = 0; i < SystemSets.Count; i++)
        {
            var set = SystemSets[i];
            var conditions = SystemSetConditions[i].ToArray();
            yield return (new NodeId(i, NodeType.Set), set, conditions);
        }
    }

    /// <summary>
    /// Returns the <see cref="PolyECS.Systems.Graph"/> of the hierarchy.
    ///
    /// The hierarchy is a directed acyclic graph of the systems and sets, where an edge denotes that a system or set is the child of another set.
    /// </summary>
    public IBidirectionalGraph<NodeId, Edge<NodeId>> GetHierarchy()
    {
        return Hierarchy;
    }

    /// <summary>
    /// Returns the <see cref="PolyECS.Systems.Graph"/> of the dependencies on the schedule.
    ///
    /// Nodes in this graph are systems and sets, and edges denote that a system or set has to run before another system or set.
    /// </summary>
    /// <returns></returns>
    public IBidirectionalGraph<NodeId, Edge<NodeId>> GetDependencyGraph()
    {
        return Dependency;
    }

    /// <summary>
    /// Returns the list of systems that conflict with each other, i.e. have ambiguities in their data access.
    ///
    /// If the <see cref="List{Type}" /> is empty, the systems conflict on <see cref="IScheduleWorld"/> access.
    /// Must be called after <see cref="BuildSchedule"/> to be non-empty.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<(NodeId, NodeId, ComponentInfo[])> GetConflictingSystems()
    {
        return ConflictingSystems.AsReadOnly();
    }

    protected ProcessConfigsResult ProcessConfig<TNode>(NodeConfig<TNode> config, bool collectNodes)
    {
        List<NodeId> nodes = new ();
        var id = config.ProcessConfig(this);
        if (collectNodes)
        {
            nodes.Add(id);
        }
        return new ProcessConfigsResult(nodes, true);
    }

    protected void ApplyCollectiveConditions<TNode>(List<NodeConfigs<TNode>> configs, List<Condition> collectiveConditions)
    {
        if (collectiveConditions.Count == 0)
        {
            return;
        }
        if (collectiveConditions.Count == 1)
        {
            foreach (var condition in collectiveConditions)
            {
                configs[0].RunIf(condition);
            }
        }
        else
        {
            var set = CreateAnonymousSet();
            foreach (var cfg in configs)
            {
                cfg.InSet(set);
            }
            var setCfg = new SystemSetConfig(set);
            setCfg.Conditions.AddRange(collectiveConditions);
            ConfigureSet(setCfg);
        }
    }

    public ProcessConfigsResult ProcessConfigs<TNode>(NodeConfigs<TNode> configs, bool collectNodes)
    {
        if (configs is NodeConfigs<TNode>.Node nodeConfig)
        {
            return ProcessConfig(nodeConfig.Config, collectNodes);
        }
        if (configs is not NodeConfigs<TNode>.Configs bundledConfigs)
            throw new ArgumentException("Invalid NodeConfigs type");

        ApplyCollectiveConditions(bundledConfigs.NodeConfigs, bundledConfigs.CollectiveConditions);
        var ignoreDeferred = bundledConfigs.Chained == Chain.YesIgnoreDeferred;
        var chained = bundledConfigs.Chained == Chain.Yes || bundledConfigs.Chained == Chain.YesIgnoreDeferred;
        var denselyChained = chained || bundledConfigs.NodeConfigs.Count == 1;
        var nodes = new List<NodeId>();

        if (bundledConfigs.NodeConfigs.Count() == 0)
        {
            return new ProcessConfigsResult(nodes, denselyChained);
        }
        var first = bundledConfigs.NodeConfigs.First();
        var previousResult = ProcessConfigs(first, collectNodes || chained);
        denselyChained &= previousResult.DenselyChained;

        foreach (var cfg in bundledConfigs.NodeConfigs.Skip(1))
        {
            var result = ProcessConfigs(cfg, collectNodes || chained);
            denselyChained &= result.DenselyChained;

            if (chained)
            {
                // if the current result is densely chained, we only need to chain the first node
                var currentNodes = result.DenselyChained ? result.Nodes[..1] : result.Nodes;
                // if the previous result was densely chained, we only need to chain the last node
                var previousNodes = previousResult.DenselyChained ? previousResult.Nodes[^1..] : previousResult.Nodes;
                foreach (var previousNode in previousNodes)
                {
                    foreach (var currentNode in currentNodes)
                    {
                        Dependency.AddEdge(new Edge<NodeId>(previousNode, currentNode));
                        if (ignoreDeferred)
                        {
                            NoSyncEdges.Add((previousNode, currentNode));
                        }
                    }
                }
            }
            if (collectNodes)
            {
                nodes.AddRange(result.Nodes);
            }
            previousResult = result;
        }

        if (collectNodes)
        {
            nodes.AddRange(previousResult.Nodes);
        }
        return new ProcessConfigsResult(nodes, denselyChained);
    }

    public NodeId AddSystem(SystemConfig config)
    {
        var id = new NodeId(Systems.Count, NodeType.System);
        UpdateGraphs(id, config.Subgraph);

        // system init has to be deferred (need `&mut World`)
        Uninit.Add((id, 0));
        Systems.Add(config.Node);
        SystemConditions.Add(config.Conditions);
        return id;
    }


    /// Add a single `SystemSetConfig` to the graph, including its dependencies and conditions.
    protected NodeId ConfigureSet(SystemSetConfig set)
    {
        var ok = SystemSetIds.TryGetValue(set.Set, out var id);
        if (!ok)
        {
            id = AddSet(set.Set);
        }

        UpdateGraphs(id, set.Subgraph);

        var conditions = SystemSetConditions[id.Id];
        Uninit.Add((id, conditions.Count));
        conditions.AddRange(set.Conditions);
        return id;
    }

    public NodeId AddSet(SystemSet set)
    {
        var id = new NodeId(Systems.Count, NodeType.Set);
        SystemSets.Add(set);
        SystemSetIds[set] = id;
        SystemSetConditions.Add(new List<Condition>());
        return id;
    }

    protected void CheckHierarchySet(NodeId id, SystemSet set)
    {
        if (SystemSetIds.TryGetValue(set, out var setId))
        {
            if (setId.Equals(id))
            {
                throw new ScheduleBuildException.HierarchyLoop(GetNodeName(setId));
            }
        }
        else
        {
            AddSet(set);
        }
    }

    /// <summary>
    /// Check that no set is included in itself.
    /// Add all the sets from the [`SubgraphInfo`]'s hierarchy to the graph.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="subgraph"></param>
    protected void CheckHierarchySets(NodeId id, SubgraphInfo subgraph)
    {
        foreach (var set in subgraph.Hierarchy)
        {
            CheckHierarchySet(id, set);
        }
    }

    protected AnonymousSet CreateAnonymousSet()
    {
        var id = AnonymousSets;
        AnonymousSets += 1;
        return new AnonymousSet(id);
    }

    /// <summary>
    /// Checks that no system set is dependent on itself.
    /// Add all the sets from the [`GraphInfo`]'s dependencies to the graph.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="subgraph"></param>
    protected void CheckEdges(NodeId id, SubgraphInfo subgraph)
    {
        // Add all the dependencies to the graph
        foreach (var dependency in subgraph.Dependencies)
        {
            if (SystemSetIds.TryGetValue(dependency.Set, out var setId))
            {
                // If a set depends on itself, throw an error
                if (setId.Equals(id))
                {
                    throw new ScheduleBuildException.DependencyLoop(GetNodeName(setId));
                }
            }
            else
            {
                AddSet(dependency.Set);
            }
        }

        // Add all the sets from the `ambiguous_with` to the graph
        if (subgraph.AmbiguousWith is Ambiguity.IgnoreWithSet ignoreWithSet)
        {
            foreach (var set in ignoreWithSet.Sets)
            {
                if (!SystemSetIds.ContainsKey(set))
                {
                    AddSet(set);
                }
            }
        }
    }

    /// <summary>
    /// Update the internal graphs (hierarchy, dependency, ambiguity) by adding a single <see cref="SubgraphInfo"/>
    /// </summary>
    /// <param name="id">Id for element being added</param>
    /// <param name="subgraph">Subgraph information about the id being added</param>
    /// <exception cref="ArgumentException">If the configuration produces an invalid subgraph an exception will be thrown</exception>
    protected void UpdateGraphs(NodeId id, SubgraphInfo subgraph)
    {
        CheckHierarchySets(id, subgraph);
        CheckEdges(id, subgraph);
        Changed = true;

        Hierarchy.AddVertex(id);
        Dependency.AddVertex(id);

        foreach (var set in subgraph.Hierarchy)
        {
            var setId = SystemSetIds[set];
            Hierarchy.AddEdge(new Edge<NodeId>(setId, id));
            // Ensure set also appears in dependency graph
            Dependency.AddVertex(setId);
        }

        foreach (var dependency in subgraph.Dependencies)
        {
            var (kind, set) = dependency;
            var (lhs, rhs) = kind switch
            {
                DependencyKind.Before => (id, SystemSetIds[set]),
                DependencyKind.BeforeNoSync => (id, SystemSetIds[set]),
                DependencyKind.After => (SystemSetIds[set], id),
                DependencyKind.AfterNoSync => (SystemSetIds[set], id),
                _ => throw new ArgumentException("Invalid dependency kind")
            };

            if (kind is DependencyKind.BeforeNoSync or DependencyKind.AfterNoSync)
            {
                NoSyncEdges.Add((lhs, rhs));
            }

            this.Dependency.AddEdge(new Edge<NodeId>(lhs, rhs));
            // Ensure set also appears in hierarchy graph
            Hierarchy.AddVertex(SystemSetIds[set]);
        }

        if (subgraph.AmbiguousWith is Ambiguity.IgnoreWithSet ignoreWithSet)
        {
            foreach (var set in ignoreWithSet.Sets)
            {
                AmbiguousWith.AddEdge(new Edge<NodeId>(id, SystemSetIds[set]));
            }
        }
        else if (subgraph.AmbiguousWith is Ambiguity.IgnoreAll)
        {
            AmbiguousWithAll.Add(id);
        }
    }

    /// Initializes any newly-added systems and conditions by calling [`System::initialize`]
    public void Initialize(World scheduleWorld)
    {
        foreach (var (id, i) in Uninit)
        {
            if (id.IsSystem)
            {
                Systems[id.Id].Initialize(scheduleWorld);
                foreach (var condition in SystemConditions)
                {
                    condition[id.Id].Initialize(scheduleWorld);
                }
            }
            else
            {
                foreach (var condition in SystemSetConditions[id.Id].Skip(i))
                {
                    condition.Initialize(scheduleWorld);
                }
            }
        }
    }

    public SystemSchedule BuildSchedule(string label, HashSet<ComponentInfo> ignoredAmbiguities)
    {
        var hierarchySort = Hierarchy.TopologicalSort().ToArray();
        var hierResults = CheckGraph(Hierarchy, hierarchySort);
        CheckHierarchyConflicts(hierResults.TransitiveEdges);

        //remove redundant edges
        Hierarchy = hierResults.TransitiveReduction;

        //check dependencies for cycles
        var dependencyTopSort = Dependency.TopologicalSort().ToArray();

        // Check for systems or sets depending on sets they belong to
        var depResults = CheckGraph(Dependency, dependencyTopSort);
        CheckCrossDependencies(depResults, hierResults.Connected);

        // Map all system sets to their systems
        // go in reverse topological order (bottom-up) for efficiency
        var (setSystems, setSystemBitsets) = MapSetsToSystems(hierarchySort, Hierarchy);
        CheckOrderButIntersect(depResults.Connected, setSystemBitsets);

        // check that there are no edges to system-type sets that have multiple instances
        CheckSystemTypeSetAmbiguity(setSystems);

        var dependencyFlattened = GetDependencyFlattened(setSystems);

        // modify graph with auto sync points
        if (Config.AutoInsertApplyDeferred)
        {
            dependencyFlattened = AutoInsertApplyDeferred(dependencyFlattened);
        }

        //topsort
        var flattenedTopsort = dependencyFlattened.TopologicalSort().ToArray();
        var flatResults = CheckGraph(dependencyFlattened, flattenedTopsort);

        //remove redundant edges
        dependencyFlattened = flatResults.TransitiveReduction;

        //flatten: combine in_set with ambiguous_with information
        var ambiguousWithFlattened = GetAmbiguousWithFlattened(setSystems);

        // check for conflicts
        var conflictingSystems = GetConflictingSystems(flatResults.Disconnected, ambiguousWithFlattened, ignoredAmbiguities);
        OptionallyCheckConflicts(conflictingSystems);
        this.ConflictingSystems = conflictingSystems;

        return BuildScheduleInner(dependencyFlattened, flattenedTopsort, hierResults.Reachable);
    }

    protected SystemSchedule BuildScheduleInner(BidirectionalGraph<NodeId, Edge<NodeId>> graph, NodeId[] topsort, FixedBitSet hierResultsReachable)
    {
        var dgSystemIdxMap = new Dictionary<NodeId, int>(topsort.Length);
        for (int i = 0; i < topsort.Length; i++)
        {
            dgSystemIdxMap[topsort[i]] = i;
        }

        var hierSort = Hierarchy.TopologicalSort().ToArray();

        var hgSystem = new List<(int, NodeId)>();
        for (int i = 0; i < hierSort.Length; i++)
        {
            if (hierSort[i].IsSystem)
            {
                hgSystem.Add((i, hierSort[i]));
            }
        }
        var hgSetWithConditionsIdxs = new List<int>();
        var hgSetIds = new List<NodeId>();
        for (int i = 0; i < hierSort.Length; i++)
        {
            // ignore system sets that have no conditions
            // ignore system type sets (already covered, they don't have conditions)
            if (hierSort[i].IsSystem || SystemSetConditions[hierSort[i].Id].Count == 0)
            {
                continue;
            }
            hgSetIds.Add(hierSort[i]);
            hgSetWithConditionsIdxs.Add(i);
        }

        var sysCount = Systems.Count;
        var setWithsConditionsCount = hgSetIds.Count;
        var hgNodeCount = Hierarchy.VertexCount;

        // get the number of dependencies and the immediate dependents of each system
        // (needed by multi_threaded executor to run systems in the correct order)
        var systemDependencies = new List<int>(sysCount);
        var systemDependents = new List<List<int>>(sysCount);
        foreach (var id in topsort)
        {
            var numDependencies = graph.InDegree(id);
            var dependents = graph.OutEdges(id).Select(x => dgSystemIdxMap[x.Target]).ToList();
            systemDependencies.Add(numDependencies);
            systemDependents.Add(dependents);
        }

        // TODO verify this logic matches bevy

        // get the rows and columns of the hierarchy graph's reachability matrix
        // (needed to we can evaluate conditions in the correct order)
        var systemsInSetsWithConditions = new List<FixedBitSet>(setWithsConditionsCount);
        for (int i = 0; i < setWithsConditionsCount; i++)
        {
            var row = hgSetWithConditionsIdxs[i];
            var bitset = new FixedBitSet(sysCount);
            foreach (var (col, sysId) in hgSystem)
            {
                var idx = dgSystemIdxMap[sysId];
                var isDescendant = hierResultsReachable[Index(row, col, hgNodeCount)];
                bitset.SetValue(idx, isDescendant);
            }
            systemsInSetsWithConditions.Add(bitset);
        }

        var setsWithConditionsOfSystems = new List<FixedBitSet>(sysCount);
        foreach (var (col, _) in hgSystem)
        {
            var bitset = new FixedBitSet(setWithsConditionsCount);
            for (int idx = 0; idx < setWithsConditionsCount; idx++)
            {
                var row = hgSetWithConditionsIdxs[idx];
                if (row >= col)
                {
                    break;
                }
                var isAncestor = hierResultsReachable[Index(row, col, hgNodeCount)];
                bitset.SetValue(idx, isAncestor);
            }
            setsWithConditionsOfSystems.Add(bitset);
        }

        return new SystemSchedule
        {
            Systems = new List<ASystem>(sysCount),
            SystemConditions = new List<List<Condition>>(sysCount),
            SetConditions = new List<List<Condition>>(setWithsConditionsCount),
            SystemIds = topsort.ToList(),
            SystemDependencies = systemDependencies,
            SystemDependents = systemDependents,
            SetsWithConditionsOfSystems = setsWithConditionsOfSystems,
            SetIds = hgSetIds,
            SystemsInSetsWithConditions = systemsInSetsWithConditions,
        };
    }

    public SystemSchedule UpdateSchedule(SystemSchedule schedule, HashSet<ComponentInfo> ignoredAmbiguities, string label)
    {
        if (Uninit.Count != 0)
        {
            throw new ScheduleBuildException.Uninitialized();
        }
        // Note - this make no sense to me. Why would this take in a new schedule instead of modifying in place

        // move systems out of old schedule
        foreach (var id in schedule.SystemIds)
        {
            Systems[id.Id] = schedule.Systems[id.Id];
            SystemConditions[id.Id] = schedule.SystemConditions[id.Id];
        }
        schedule.SystemIds.Clear();
        schedule.Systems.Clear();
        schedule.SystemConditions.Clear();

        foreach (var id in schedule.SetIds)
        {
            SystemSetConditions[id.Id] = schedule.SetConditions[id.Id];
        }
        schedule.SetIds.Clear();
        schedule.SetConditions.Clear();

        var newSchedule = BuildSchedule(label, ignoredAmbiguities);

        // move systems into new schedule
        foreach (var id in schedule.SystemIds)
        {
            var system = Systems[id.Id];
            var conditions = SystemConditions[id.Id];
            newSchedule.Systems.Add(system);
            newSchedule.SystemConditions.Add(conditions);
        }
        foreach (var id in schedule.SetIds)
        {
            var conditions = SystemSetConditions[id.Id];
            newSchedule.SetConditions.Add(conditions);
        }
        return newSchedule;
    }

    private List<(NodeId, NodeId, ComponentInfo[])> GetConflictingSystems(
        List<(NodeId, NodeId)> flatResultsDisconnected,
        UndirectedGraph<NodeId, Edge<NodeId>> ambiguousWithFlattened,
        HashSet<ComponentInfo> ignoredAmbiguities
    )
    {
        var conflictingSystems = new List<(NodeId, NodeId, ComponentInfo[])>();
        foreach (var (a, b) in flatResultsDisconnected)
        {
            if (ambiguousWithFlattened.ContainsEdge(a, b) || AmbiguousWithAll.Contains(a) || AmbiguousWithAll.Contains(b))
            {
                continue;
            }
            var systemA = Systems[a.Id];
            var systemB = Systems[b.Id];

            if (systemA.IsExclusive || systemB.IsExclusive)
            {
                conflictingSystems.Add((a, b, []));
            }
            else
            {
                var aAccess = systemA.GetAccess();
                var bAccess = systemB.GetAccess();
                if (!aAccess.IsCompatible(bAccess))
                {
                    var conflicts = aAccess.GetConflicts(bAccess).Where(x => !ignoredAmbiguities.Contains(x)).ToArray();
                    if (conflicts.Length > 0)
                    {
                        conflictingSystems.Add((a, b, conflicts));
                    }
                }
            }
        }
        return conflictingSystems;
    }

    private List<NodeId> ResolveNodeToSystems(Dictionary<NodeId, List<NodeId>> setSystems, NodeId id)
    {
        if (id.IsSystem)
        {
            return new List<NodeId>
            {
                id
            };
        }
        return setSystems.GetValueOrDefault(id, new List<NodeId>());
    }

    private UndirectedGraph<NodeId, Edge<NodeId>> GetAmbiguousWithFlattened(Dictionary<NodeId, List<NodeId>> setSystems)
    {
        var graph = new UndirectedGraph<NodeId, Edge<NodeId>>();
        foreach (var edge in AmbiguousWith.Edges)
        {
            foreach (var source in ResolveNodeToSystems(setSystems, edge.Source))
            {
                foreach (var target in ResolveNodeToSystems(setSystems, edge.Target))
                {
                    graph.AddEdge(new Edge<NodeId>(source, target));
                }
            }
        }
        return graph;
    }

    /// <summary>
    /// Modify the graph to have sync nodes for any dependants after a system with deferred system params
    /// </summary>
    /// <param name="flattened"></param>
    /// <returns></returns>
    protected BidirectionalGraph<NodeId, Edge<NodeId>> AutoInsertApplyDeferred(BidirectionalGraph<NodeId, Edge<NodeId>> flattened)
    {
        var syncGraph = flattened.Clone();
        var topo = flattened.TopologicalSort().ToArray();

        // calculate the number of sync points each sync point is from the beginning of the graph
        // use the same sync point if the distance is the same
        var distances = new Dictionary<int, uint>(topo.Length);
        foreach (var node in topo)
        {
            var addSyncAfter = Systems[node.Id].HasDeferred;
            var nodeDist = distances.GetValueOrDefault<int, uint>(node.Id, 0);

            foreach (var edge in flattened.OutEdges(node))
            {
                var addSyncOnEdge = addSyncAfter && !NoSyncEdges.Contains((node, edge.Target)) && Systems[edge.Target.Id] is not ApplyDeferredSystem;
                var weight = (uint)(addSyncOnEdge ? 1 : 0);
                var dist = distances.GetValueOrDefault<int, uint>(edge.Target.Id, 0);
                dist = dist > nodeDist + weight ? dist : nodeDist + weight;

                distances[edge.Target.Id] = dist;

                if (addSyncOnEdge)
                {
                    var syncPoint = GetSyncPoint(distances[edge.Target.Id]);
                    syncGraph.AddEdge(new Edge<NodeId>(node, syncPoint));
                    syncGraph.AddEdge(new Edge<NodeId>(syncPoint, edge.Target));

                    //edge is now redundant
                    syncGraph.RemoveEdge(edge);
                }
            }
        }

        return syncGraph;
    }

    protected NodeId GetSyncPoint(uint dist)
    {
        if (AutoSyncNodeIds.TryGetValue(dist, out var id))
        {
            return id;
        }
        id = AddAutoSync();
        AutoSyncNodeIds.Add(dist, id);
        return id;
    }

    /// <summary>
    /// Adds a <see cref="ApplyDeferredSystem"/> with no config
    /// </summary>
    /// <returns>Id of sync system</returns>
    protected NodeId AddAutoSync()
    {
        var id = new NodeId(AutoSyncNodeIds.Count, NodeType.System);
        Systems.Add(new ApplyDeferredSystem());
        SystemConditions.Add(new List<Condition>());
        // ignore ambiguities with auto sync points
        // They aren't under user control, so no one should know or care.
        AmbiguousWithAll.Add(id);
        return id;
    }

    protected BidirectionalGraph<NodeId, Edge<NodeId>> GetDependencyFlattened(Dictionary<NodeId, List<NodeId>> setSystems)
    {
        // flatten: combine `in_set` with `before` and `after` information
        // have to do it like this to preserve transitivity
        var flattened = Dependency.Clone();
        var temp = new List<(NodeId, NodeId)>();
        foreach (var (set, systems) in setSystems)
        {
            if (systems.Count == 0)
            {
                // collapse dependencies for empty sets
                foreach (var aEdge in flattened.InEdges(set))
                {
                    foreach (var bEdge in flattened.OutEdges(set))
                    {
                        if (NoSyncEdges.Contains((aEdge.Source, set)) && NoSyncEdges.Contains((set, bEdge.Target)))
                        {
                            NoSyncEdges.Add((aEdge.Source, bEdge.Target));
                        }
                        temp.Add((aEdge.Source, bEdge.Target));
                    }
                }
            }
            else
            {
                foreach (var aEdge in flattened.InEdges(set))
                {
                    foreach (var sys in systems)
                    {
                        if (NoSyncEdges.Contains((aEdge.Source, set)))
                        {
                            NoSyncEdges.Add((aEdge.Source, sys));
                        }
                        temp.Add((aEdge.Source, sys));
                    }
                }

                foreach (var bEdge in flattened.OutEdges(set))
                {
                    foreach (var sys in systems)
                    {
                        if (NoSyncEdges.Contains((set, bEdge.Target)))
                        {
                            NoSyncEdges.Add((sys, bEdge.Target));
                        }
                        temp.Add((sys, bEdge.Target));
                    }
                }
            }

            flattened.RemoveVertex(set);
            foreach (var (a, b) in temp)
            {
                flattened.AddEdge(new Edge<NodeId>(a, b));
            }
            temp.Clear();
        }

        return flattened;
    }

    /// <summary>
    /// Return a map from a <see cref="SystemSet"/> <see cref="NodeId"/> to a list of <see cref="ASystem"/> <see cref="NodeId"/>'s that are included in the set.
    /// Also return a map from a <see cref="SystemSet"/> <see cref="NodeId"/> to a <see cref="FixedBitSet"/> of <see cref="SystemSet"/> <see cref="NodeId"/>'s
    /// that are included in the set, where the bitset order is the same as <see cref="SystemGraph.GetSystems()"/>
    /// </summary>
    /// <param name="hierarchyTopsort"></param>
    /// <param name="hierarchyGraph"></param>
    /// <returns></returns>
    protected (Dictionary<NodeId, List<NodeId>>, Dictionary<NodeId, FixedBitSet>) MapSetsToSystems(
        NodeId[] hierarchyTopsort,
        BidirectionalGraph<NodeId, Edge<NodeId>> hierarchyGraph
    )
    {
        var setSystems = new Dictionary<NodeId, List<NodeId>>(SystemSets.Count);
        var setSystemBitsets = new Dictionary<NodeId, FixedBitSet>(SystemSets.Count);

        for (int i = hierarchyTopsort.Count() - 1; i >= 0; i--)
        {
            var id = hierarchyTopsort[i];
            if (id.IsSystem)
            {
                continue;
            }
            var systems = new List<NodeId>();
            var systemBitset = new FixedBitSet(systems.Count);
            foreach (var edge in hierarchyGraph.OutEdges(id))
            {
                if (edge.Target.IsSystem)
                {
                    systems.Add(edge.Target);
                    systemBitset.Set(edge.Target.Id);
                }
                else
                {
                    if (setSystems.TryGetValue(edge.Target, out var childSystems))
                    {
                        systems.AddRange(childSystems);
                    }
                    if (setSystemBitsets.TryGetValue(edge.Target, out var childSystemBitset))
                    {
                        systemBitset.Or(childSystemBitset);
                    }
                }
            }

            setSystems[id] = systems;
            setSystemBitsets[id] = systemBitset;
        }
        return (setSystems, setSystemBitsets);
    }

    public String GetNodeName(NodeId id)
    {
        return id.Type == NodeType.System ? Systems[id.Id].GetType().Name : SystemSets[id.Id].GetType().Name;
    }

    void CheckCrossDependencies(CheckGraphResults<NodeId> depResults, HashSet<(NodeId, NodeId)> hierResultsConnected)
    {
        foreach (var (a, b) in depResults.Connected)
        {
            if (hierResultsConnected.Contains((a, b)) || hierResultsConnected.Contains((b, a)))
            {
                throw new ScheduleBuildException.CrossDependency(GetNodeName(a), GetNodeName(b));
            }
        }
    }

    /// <summary>
    /// Processes a DAG and computes its:
    /// <list type="bullet">
    /// <item> transitive reduction (along with the set of removed edges) </item>
    /// <item> transitive closure </item>
    /// <item> reachability matrix (as a bitset) </item>
    /// <item> pairs of nodes connected by a path </item>
    /// <item> pairs of nodes not connected by a path </item>
    /// </list>
    /// 
    /// The algorithm implemented comes from "On the calculation of transitive reduction-closure of orders" by Habib, Morvan and Rampon. 
    /// </summary>
    /// <seealso href="https://doi.org/10.1016/0012-365X(93)90164-O"/>
    /// <remarks>
    /// Port of bevy_ecs::schedule::graph_utils::check_graph
    ///</remarks>
    public static CheckGraphResults<TV> CheckGraph<TV>(IBidirectionalGraph<TV, Edge<TV>> graph, TV[] topologicalOrder) where TV : notnull
    {
        var n = graph.VertexCount;
        if (n == 0)
        {
            return new CheckGraphResults<TV>();
        }

        // build a copy of the graph where the nodes and edges appear in topsorted order
        var map = new Dictionary<TV, int>(n);
        var topsorted = new BidirectionalGraph<TV, Edge<TV>>();

        // iterate nodes in topological order
        for (int i = 0; i < n; i++)
        {
            map[topologicalOrder[i]] = i;
            topsorted.AddVertex(topologicalOrder[i]);
            // insert nodes as successors to their predecessors
            foreach (var edge in graph.InEdges(topologicalOrder[i]))
            {
                topsorted.AddEdge(new Edge<TV>(edge.Source, edge.Target));
            }
        }

        var reachable = new FixedBitSet(n * n);
        var connected = new HashSet<(TV, TV)>();
        var disconnected = new List<(TV, TV)>();

        var transitiveEdges = new List<(TV, TV)>();
        var transitiveReduction = new BidirectionalGraph<TV, Edge<TV>>();
        var transitiveClosure = new BidirectionalGraph<TV, Edge<TV>>();

        var visited = new FixedBitSet(n);
        foreach (var vertex in topsorted.Vertices)
        {
            transitiveReduction.AddVertex(vertex);
            transitiveClosure.AddVertex(vertex);
        }

        // iterate nodes in reverse topological order
        // Note - Bevy does this by iterating the topsort vertices, but our graph implementation doesn't support ordered insertion
        for (int indexA = n - 1; indexA >= 0; indexA--)
        {
            var a = topologicalOrder[indexA];
            foreach (var edge in topsorted.OutEdges(a))
            {
                var b = edge.Target;
                var indexB = map[b];
                if (indexA >= indexB)
                {
                    throw new UnreachableException("topological order is not valid");
                }
                if (!visited[indexB])
                {
                    transitiveReduction.AddEdge(new Edge<TV>(a, b));
                    transitiveClosure.AddEdge(new Edge<TV>(a, b));
                    reachable.Set(indexA * n + indexB);

                    foreach (var c in transitiveClosure.OutEdges(b).Select(x => x.Target))
                    {
                        var indexC = map[c];
                        if (!visited[indexC])
                        {
                            visited.Set(indexC);
                            transitiveClosure.AddEdge(new Edge<TV>(a, c));
                            reachable.Set(indexA * n + indexC);
                        }
                    }

                }
                else
                {
                    // edge <a, b> is redundant
                    transitiveEdges.Add((a, b));
                }
            }
            visited.Clear();
        }

        // partition pairs of nodes into "connected by path" and "not connected by path"
        for (int i = 0; i < n; i++)
        {
            // reachable is upper triangular because the nodes were topsorted
            for (int index = Index(i, i + 1, n); index <= Index(i, n - 1, n); index++)
            {
                var (a, b) = RowCol(index, n);
                var pair = (topologicalOrder[a], topologicalOrder[b]);
                if (reachable[index])
                {
                    connected.Add(pair);
                }
                else
                {
                    disconnected.Add(pair);
                }
            }
        }

        return new CheckGraphResults<TV>
        {
            Reachable = reachable,
            Connected = connected,
            Disconnected = disconnected,
            TransitiveEdges = transitiveEdges,
            TransitiveReduction = transitiveReduction,
            TransitiveClosure = transitiveClosure
        };
    }

    /// <summary>
    /// Converts 2D row-major pair of indices into a 1D array index.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="numCols"></param>
    /// <returns></returns>
    private static int Index(int row, int col, int numCols)
    {
        return row * numCols + col;
    }

    /// <summary>
    /// Converts a 1D array index into a 2D row-major pair of indices.
    /// </summary>
    private static (int, int) RowCol(int index, int numCols)
    {
        return (index / numCols, index % numCols);
    }

    void CheckHierarchyConflicts(List<(NodeId, NodeId)> transitiveEdges)
    {
        if (!Config.ThrowHierarchyRedundancyErrors || transitiveEdges.Count == 0)
        {
            return;
        }

        var message = "hierarchy contains redundant edge(s)";
        foreach (var (parent, child) in transitiveEdges)
        {
            message += $"\n -- {child.Type} `{GetNodeName(child)}` cannot be child of set `{GetNodeName(parent)}`, longer path exists";
        }
        throw new ScheduleBuildException.HierarchyRedundancy(message);
    }


    void CheckOrderButIntersect(HashSet<(NodeId, NodeId)> depResultsConnected, Dictionary<NodeId, FixedBitSet> setSystemBitsets)
    {
        foreach (var (a, b) in depResultsConnected)
        {
            if (!(a.IsSet && b.IsSet))
            {
                continue;
            }
            var aSystems = setSystemBitsets[a];
            var bSystems = setSystemBitsets[b];
            if (!aSystems.IsDisjoint(bSystems))
            {
                throw new ScheduleBuildException.SetsHaveOrderButIntersect(GetNodeName(a), GetNodeName(b));
            }
        }
    }

    void CheckSystemTypeSetAmbiguity(Dictionary<NodeId, List<NodeId>> setSystems)
    {
        foreach (var (id, systems) in setSystems)
        {
            var set = SystemSets[id.Id];
            if (set.SystemType() != null)
            {
                var instances = systems.Count;
                var ambiguous = AmbiguousWith.AdjacentEdges(id);
                var before = Dependency.InDegree(id);
                var after = Dependency.OutDegree(id);
                var relations = before + after + ambiguous.Count();
                if (instances > 1 && relations > 0)
                {
                    throw new ScheduleBuildException.SystemTypeSetAmbiguity(GetNodeName(id));
                }
            }
        }
    }

    void OptionallyCheckConflicts(List<(NodeId, NodeId, ComponentInfo[])> conflicts)
    {
        if (Config.ThrowAmbiguousErrors)
        {
            if (conflicts.Count > 0)
            {
                var message = GetConflictsErrorMessage(conflicts);
                throw new ScheduleBuildException.Ambiguity(message);
            }
        }
    }
    

    string GetConflictsErrorMessage(List<(NodeId, NodeId, ComponentInfo[])> ambiguities)
    {
        var nAmbiguities = ambiguities.Count;
        var message = $"{nAmbiguities} pairs of systems with conflicting data access have indeterminate execution order.\n" +
                      $"Consider adding `before`, `after`, or `ambiguous_with` relationships between these:\n";
        foreach (var (nameA, nameB, conflicts) in ambiguities)
        {
            message += $" -- {nameA} and {nameB}\n";
            if (conflicts.Length != 0)
            {
                // TODO name
                message += $"    conflict on: {string.Join(", ", conflicts.Select(c => c.ID))}\n";
            }
            else
            {
                message += $"    conflict on: World\n";
            }
        }
        return message;
    }
}
