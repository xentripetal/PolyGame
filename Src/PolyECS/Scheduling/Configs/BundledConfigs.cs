using PolyECS.Scheduling.Graph;
using PolyECS.Systems;

namespace PolyECS.Scheduling.Configs;

public abstract class SystemConfigs : NodeConfigs<ISystem> { }

public abstract class SetConfigs : NodeConfigs<ISystemSet>
{
    public static NodeConfigs<ISystemSet> Of<T>(params T[] sets) where T : struct, Enum
    {
        var convertedSets = new IIntoNodeConfigs<ISystemSet>[sets.Length];
        for (var i = 0; i < sets.Length; i++)
        {
            convertedSets[i] = new EnumSystemSet<T>(sets[i]);
        }
        return NodeConfigs<ISystemSet>.Of(convertedSets);
    }
}

/// <summary>
///     A collection of generic <see cref="NodeConfig{T}" />s
///     A port of bevy_ecs::schedule::config::NodeConfigs
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class NodeConfigs<T> : IIntoNodeConfigs<T>
{
    public NodeConfigs<T> IntoConfigs() => this;

    /// <summary>
    ///     Adds a system set to the systems
    /// </summary>
    /// <param name="set"></param>
    public abstract NodeConfigs<T> InSet(IIntoSystemSet set);

    /// <summary>
    ///     Adds a system to the set represented by the enum
    /// </summary>
    /// <param name="set"></param>
    /// <typeparam name="TEnum"></typeparam>
    public NodeConfigs<T> InSet<TEnum>(TEnum set) where TEnum : struct, Enum => InSet(new EnumSystemSet<TEnum>(set));

    public abstract NodeConfigs<T> Before(IIntoSystemSet set);

    public abstract NodeConfigs<T> After(IIntoSystemSet set);

    public abstract NodeConfigs<T> BeforeIgnoreDeferred(IIntoSystemSet set);

    public abstract NodeConfigs<T> AfterIgnoreDeferred(IIntoSystemSet set);

    public abstract NodeConfigs<T> DistributiveRunIf(ICondition condition);

    public abstract NodeConfigs<T> AmbiguousWith(IIntoSystemSet set);

    public abstract NodeConfigs<T> AmbiguousWithAll();

    public abstract NodeConfigs<T> RunIf(ICondition condition);

    public abstract NodeConfigs<T> Chained();

    public abstract NodeConfigs<T> ChainedIgnoreDeferred();

    public static NodeConfigs<T> Of(NodeConfig<T> config) => new Node(config);

    public static NodeConfigs<T> Of(params IIntoNodeConfigs<T>[] configs) => Of(configs, null, Chain.No);

    public static NodeConfigs<T> Of(IIntoNodeConfigs<T>[] configs, ICondition[]? collectiveConditions = null, Chain chained = Chain.No)
    {
        if (configs == null || configs.Length == 0)
        {
            throw new ArgumentException("NodeConfigs must not be empty");
        }


        var convertedConfigs = new NodeConfigs<T>[configs.Length];
        for (var i = 0; i < configs.Length; i++)
        {
            convertedConfigs[i] = configs[i].IntoConfigs();
        }
        return new Configs(convertedConfigs, collectiveConditions, chained);
    }


    public class Node(NodeConfig<T> config) : NodeConfigs<T>
    {
        public readonly NodeConfig<T> Config = config;

        public override NodeConfigs<T> InSet(IIntoSystemSet set)
        {
            Config.Subgraph.Hierarchy.Add(set.IntoSystemSet());
            return this;
        }

        public override NodeConfigs<T> Before(IIntoSystemSet set)
        {
            Config.Subgraph.Dependencies.Add(new Dependency(DependencyKind.Before, set.IntoSystemSet()));
            return this;
        }

        public override NodeConfigs<T> After(IIntoSystemSet set)
        {
            Config.Subgraph.Dependencies.Add(new Dependency(DependencyKind.After, set.IntoSystemSet()));
            return this;
        }

        public override NodeConfigs<T> BeforeIgnoreDeferred(IIntoSystemSet set)
        {
            Config.Subgraph.Dependencies.Add(new Dependency(DependencyKind.BeforeNoSync, set.IntoSystemSet()));
            return this;
        }

        public override NodeConfigs<T> AfterIgnoreDeferred(IIntoSystemSet set)
        {
            Config.Subgraph.Dependencies.Add(new Dependency(DependencyKind.AfterNoSync, set.IntoSystemSet()));
            return this;
        }

        public override NodeConfigs<T> DistributiveRunIf(ICondition condition)
        {
            Config.Conditions.Add(condition);
            return this;
        }

        public override NodeConfigs<T> AmbiguousWith(IIntoSystemSet set)
        {
            Config.Subgraph.AddAmbiguousWith(set.IntoSystemSet());
            return this;
        }

        public override NodeConfigs<T> AmbiguousWithAll()
        {
            Config.Subgraph.AmbiguousWith = new Ambiguity.IgnoreAll();
            return this;
        }

        public override NodeConfigs<T> RunIf(ICondition condition)
        {
            Config.Conditions.Add(condition);
            return this;
        }

        public override NodeConfigs<T> Chained() =>
            //no-op
            this;

        public override NodeConfigs<T> ChainedIgnoreDeferred() =>
            //no-op
            this;
    }

    public class Configs : NodeConfigs<T>
    {
        /// <summary>
        ///     Configurations for each element of the tuple
        /// </summary>
        public readonly NodeConfigs<T>[] NodeConfigs;
        /// <summary>
        ///     See <see cref="Chain" /> for usage.
        /// </summary>
        public Chain Chain;
        /// <summary>
        ///     Run conditions applied to everything in the tuple.
        /// </summary>
        public List<ICondition> CollectiveConditions;

        public Configs(NodeConfigs<T>[] nodeConfigs, ICondition[]? collectiveConditions, Chain chain)
        {
            if (nodeConfigs == null || nodeConfigs.Length == 0)
            {
                throw new ArgumentException("NodeConfigs must not be empty");
            }
            NodeConfigs = nodeConfigs;
            CollectiveConditions = collectiveConditions?.ToList() ?? [];
            Chain = chain;
        }

        public override NodeConfigs<T> InSet(IIntoSystemSet set)
        {
            foreach (var cfg in NodeConfigs)
            {
                cfg.InSet(set);
            }
            return this;
        }

        public override NodeConfigs<T> Before(IIntoSystemSet set)
        {
            foreach (var cfg in NodeConfigs)
            {
                cfg.Before(set);
            }
            return this;
        }

        public override NodeConfigs<T> After(IIntoSystemSet set)
        {
            foreach (var cfg in NodeConfigs)
            {
                cfg.After(set);
            }
            return this;
        }

        public override NodeConfigs<T> BeforeIgnoreDeferred(IIntoSystemSet set)
        {
            foreach (var cfg in NodeConfigs)
            {
                cfg.BeforeIgnoreDeferred(set);
            }
            return this;
        }

        public override NodeConfigs<T> AfterIgnoreDeferred(IIntoSystemSet set)
        {
            foreach (var cfg in NodeConfigs)
            {
                cfg.AfterIgnoreDeferred(set);
            }
            return this;
        }

        public override NodeConfigs<T> DistributiveRunIf(ICondition condition)
        {
            CollectiveConditions.Add(condition);
            return this;
        }

        public override NodeConfigs<T> AmbiguousWith(IIntoSystemSet set)
        {
            foreach (var cfg in NodeConfigs)
            {
                cfg.AmbiguousWith(set);
            }
            return this;
        }

        public override NodeConfigs<T> AmbiguousWithAll()
        {
            foreach (var cfg in NodeConfigs)
            {
                cfg.AmbiguousWithAll();
            }
            return this;
        }

        public override NodeConfigs<T> RunIf(ICondition condition)
        {
            foreach (var cfg in NodeConfigs)
            {
                cfg.RunIf(condition);
            }
            return this;
        }

        public override NodeConfigs<T> Chained()
        {
            Chain = Chain.Yes;
            return this;
        }

        public override NodeConfigs<T> ChainedIgnoreDeferred()
        {
            Chain = Chain.YesIgnoreDeferred;
            return this;
        }
    }
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
