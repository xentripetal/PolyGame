using System.Diagnostics.CodeAnalysis;

namespace PolyScheduler;

public interface IConfigurableScheduleEntry<T, TContext, TResource> where TContext : IContext<TContext> {
    /// <summary>
    ///     Adds a system set to the systems
    /// </summary>
    /// <param name="set"></param>
    public IConfigurableScheduleEntry<T, TContext, TResource> InSet(ISystemSet set);

    /// <summary>
    ///     Adds a system to the set represented by the enum
    /// </summary>
    /// <param name="set"></param>
    /// <typeparam name="TEnum"></typeparam>
    public IConfigurableScheduleEntry<T, TContext, TResource> InSet<TEnum>(TEnum set) where TEnum : struct, Enum =>
        InSet(new EnumSystemSet<TEnum>(set));

    public IConfigurableScheduleEntry<T, TContext, TResource> Before(ISystemSet set);

    public IConfigurableScheduleEntry<T, TContext, TResource> After(ISystemSet set);

    public IConfigurableScheduleEntry<T, TContext, TResource> BeforeIgnoreDeferred(ISystemSet set);

    public IConfigurableScheduleEntry<T, TContext, TResource> AfterIgnoreDeferred(ISystemSet set);

    public IConfigurableScheduleEntry<T, TContext, TResource> DistributiveRunIf(ICondition<TContext> condition);

    public IConfigurableScheduleEntry<T, TContext, TResource> AmbiguousWith(ISystemSet set);

    public IConfigurableScheduleEntry<T, TContext, TResource> AmbiguousWithAll();

    public IConfigurableScheduleEntry<T, TContext, TResource> RunIf(params List<ICondition<TContext>> condition);

    public IConfigurableScheduleEntry<T, TContext, TResource> Chained();

    public IConfigurableScheduleEntry<T, TContext, TResource> ChainedIgnoreDeferred();

    public AddEntryResult AddToGraph(ScheduleGraph<TContext, TResource> graph);
}

/// <summary>
///     Stores configuration for a single generic node (a system or a system set)
///     The configuration includes the node itself, scheduling metadata
///     (hierarchy: in which sets is the node contained,
///     dependencies: before/after which other nodes should this node run)
///     and the run conditions associated with this node.
///     Port of bevy_ecs::schedule::config::NodeConfig
/// </summary>
[method: SetsRequiredMembers]
public abstract class ScheduleEntry<TNode, TContext, TResource>(TNode node)
    : IConfigurableScheduleEntry<TNode, TContext, TResource> where TContext : IContext<TContext>
{
    public required TNode Node = node;

    public Ambiguity Ambiguity = new Ambiguity.Check();

    public readonly List<Dependency> Dependencies = new();

    public readonly List<ISystemSet> Hierarchy = new();

    public abstract AddEntryResult AddToGraph(ScheduleGraph<TContext, TResource> graph);

    /// <summary>
    ///     Marks the given set as ambiguous with this node. If the node is already marked as globally ambiguous, this does
    ///     nothing.
    /// </summary>
    /// <param name="set"></param>
    public void AddAmbiguousWith(ISystemSet set)
    {
        if (Ambiguity is Ambiguity.IgnoreWithSet ignoreWithSet)
        {
            ignoreWithSet.Sets.Add(set);
        }
        else if (Ambiguity is Ambiguity.Check)
        {
            Ambiguity = new Ambiguity.IgnoreWithSet([set]);
        }
    }


    public readonly List<ICondition<TContext>> Conditions = new();

    public IConfigurableScheduleEntry<TNode, TContext, TResource> InSet(ISystemSet set)
    {
        Hierarchy.Add(set);
        return this;
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> Before(ISystemSet set)
    {
        Dependencies.Add(new Dependency(Dependency.Kind.Before, set));
        return this;
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> After(ISystemSet set)
    {
        Dependencies.Add(new Dependency(Dependency.Kind.After, set));
        return this;
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> BeforeIgnoreDeferred(ISystemSet set)
    {
        Dependencies.Add(new Dependency(Dependency.Kind.BeforeNoSync, set));
        return this;
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> AfterIgnoreDeferred(ISystemSet set)
    {
        Dependencies.Add(new Dependency(Dependency.Kind.AfterNoSync, set));
        return this;
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> DistributiveRunIf(ICondition<TContext> condition)
    {
        Conditions.Add(condition);
        return this;
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> AmbiguousWith(ISystemSet set)
    {
        AddAmbiguousWith(set);
        return this;
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> AmbiguousWithAll()
    {
        Ambiguity = new Ambiguity.IgnoreAll();
        return this;
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> RunIf(params List<ICondition<TContext>> conditions)
    {
        Conditions.AddRange(conditions);
        return this;
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> Chained()
    {
        //no-op
        return this;
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> ChainedIgnoreDeferred()
    {
        //no-op
        return this;
    }
}

[method: SetsRequiredMembers]
public class SystemEntry<TContext, TResource>
    : ScheduleEntry<ISystem<TContext>, TContext, TResource> where TContext : IContext<TContext>
{
    [SetsRequiredMembers]
    public SystemEntry(ISystem<TContext> system) : base(system) {
        Hierarchy.Add(system.SetAlias);
    }
    public override AddEntryResult AddToGraph(ScheduleGraph<TContext, TResource> graph)
    {
        var id = graph.AddSystem(this);
        return new AddEntryResult([id], true);
    }
}

[method: SetsRequiredMembers]
public class SetEntry<TContext, TResource>(ISystemSet set) : ScheduleEntry<ISystemSet, TContext, TResource>(set) where TContext : IContext<TContext>
{
    public override AddEntryResult AddToGraph(ScheduleGraph<TContext, TResource> graph)
    {
        var id = graph.ConfigureSet(this);
        return new AddEntryResult([id], true);
    }
}

public class ScheduleEntries<TNode, TContext, TResource> : IConfigurableScheduleEntry<TNode, TContext, TResource> where TContext : IContext<TContext>
{
    protected readonly List<IConfigurableScheduleEntry<TNode, TContext, TResource>> Entries = new();

    public IReadOnlyCollection<IConfigurableScheduleEntry<TNode, TContext, TResource>> GetEntries() => Entries;
    public Chain Chain { get; protected set; }
    protected readonly List<ICondition<TContext>> Conditions = new();

    public ScheduleEntries(IConfigurableScheduleEntry<TNode, TContext, TResource> entries,
        List<ICondition<TContext>>? conditions = null, Chain chain = Chain.No)
    {
        Entries.Add(entries);
        if (conditions != null)
        {
            Conditions = conditions;
        }

        Chain = chain;
    }

    protected IConfigurableScheduleEntry<TNode, TContext, TResource> ApplyAll(
        Action<IConfigurableScheduleEntry<TNode, TContext, TResource>> action)
    {
        foreach (var entry in Entries)
        {
            action(entry);
        }

        return this;
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> InSet(ISystemSet set)
    {
        return ApplyAll(x => x.InSet(set));
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> Before(ISystemSet set)
    {
        return ApplyAll(x => x.Before(set));
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> After(ISystemSet set)
    {
        return ApplyAll(x => x.After(set));
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> BeforeIgnoreDeferred(ISystemSet set)
    {
        return ApplyAll(x => x.BeforeIgnoreDeferred(set));
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> AfterIgnoreDeferred(ISystemSet set)
    {
        return ApplyAll(x => x.AfterIgnoreDeferred(set));
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> DistributiveRunIf(ICondition<TContext> condition)
    {
        Conditions.Add(condition);
        return this;
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> AmbiguousWith(ISystemSet set)
    {
        return ApplyAll(x => x.AmbiguousWith(set));
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> AmbiguousWithAll()
    {
        return ApplyAll(x => x.AmbiguousWithAll());
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> RunIf(params List<ICondition<TContext>> conditions)
    {
        return ApplyAll(x => x.RunIf(conditions));
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> Chained()
    {
        Chain = Chain.Yes;
        return this;
    }

    public IConfigurableScheduleEntry<TNode, TContext, TResource> ChainedIgnoreDeferred()
    {
        Chain = Chain.YesIgnoreDeferred;
        return this;
    }

    public AddEntryResult AddToGraph(ScheduleGraph<TContext, TResource> graph)
    {
        ReduceConditions();
        foreach (var entry in AnonymousEntries)
        {
            entry.AddToGraph(graph);
        }

        var ignoreDeferred = Chain == Chain.YesIgnoreDeferred;
        var chained = Chain != Chain.No;
        var denselyChained = chained || Entries.Count == 1;

        var nodes = new List<NodeId>();
        if (Entries.Count == 0)
        {
            return new AddEntryResult(nodes, denselyChained);
        }

        var first = Entries.First();
        var previousResult = first.AddToGraph(graph);
        denselyChained &= previousResult.DenselyChained;

        foreach (var entry in Entries.Skip(1))
        {
            var result = entry.AddToGraph(graph);
            denselyChained &= result.DenselyChained;
            if (chained)
            {
                // if the current result is densely chained, we only need to chain the first node
                var currentNodes = result.DenselyChained ? result.Nodes[..1] : result.Nodes;
                // if the previous result was densely chained, we only need to chain the last node
                var previousNodes = previousResult.DenselyChained ? previousResult.Nodes[^1..] : previousResult.Nodes;
                previousNodes.ForEach(prev =>
                    currentNodes.ForEach(cur => graph.AddDependency(prev, cur, ignoreDeferred)));
            }

            nodes.AddRange(result.Nodes);
            previousResult = result;
        }

        return new AddEntryResult(nodes, denselyChained);
    }

    /// <summary>
    /// Reduces the shared conditions to <see cref="ScheduleEntry{TNode,TContext,TResource}"/> conditions. 
    /// </summary>
    protected void ReduceConditions()
    {
        if (Conditions.Count == 0)
            return;
        // If there is only one entry, just transfer the conditions to it.
        if (Entries.Count == 1)
        {
            Entries[0].RunIf(Conditions);
        }
        // Add the conditions to a single set and push all the entries onto that set.
        else
        {
            var set = new AnonymousSystemSet();
            foreach (var entry in Entries)
            {
                entry.InSet(set);
            }

            var setEntry = new SetEntry<TContext, TResource>(set);
            setEntry.RunIf(Conditions);
            AnonymousEntries.Add(setEntry);
        }

        // Make sure the conditions are cleared so they are not applied again
        Conditions.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    protected readonly List<SetEntry<TContext, TResource>> AnonymousEntries = new();
}

public enum Chain
{
    /// <summary>
    ///     Run nodes in order. If there are deferred parameters in preceding systems a ApplyDeferred will be added on the edge
    /// </summary>
    Yes,

    /// <summary>
    ///     Run nodes in order. This will not add ApplyDeferred on the edge
    /// </summary>
    YesIgnoreDeferred,

    // Nodes are allowed to run in any order
    No
}