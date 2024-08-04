using PolyECS.Scheduling.Graph;
using PolyECS.Systems;
using PolyECS.Systems.Graph;

namespace PolyECS.Scheduling.Configs;

public abstract class SystemConfigs : NodeConfigs<RunSystem>, IIntoSystemConfigs
{
    public static SystemConfigs Of(IIntoSystemConfigs[] systems, Condition[]? collectiveConditions = null, Chain chained = Chain.No)
    {
        var configs = new NodeConfigs<RunSystem>[systems.Length];
        for (var i = 0; i < systems.Length; i++)
        {
            configs[i] = systems[i].IntoSystemConfig();
        }
        return (SystemConfigs)Of(configs, collectiveConditions, chained);
    }

    public static SystemConfigs Of(params IIntoSystemConfigs[] configs)
    {
        return Of(configs, null, Chain.No);
    }

    public SystemConfigs IntoSystemConfig() => this;
}

public abstract class SetConfigs : NodeConfigs<ISystemSet> { }

/// <summary>
/// A collection of generic <see cref="NodeConfig{T}"/>s
///
/// A port of bevy_ecs::schedule::config::NodeConfigs
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class NodeConfigs<T>
{
    public static NodeConfigs<T> Of(NodeConfig<T> config)
    {
        return new Node(config);
    }


    public static NodeConfigs<T> Of(NodeConfig<T>[] configs, Condition[]? collectiveConditions = null, Chain chained = Chain.No)
    {
        if (configs == null || configs.Length == 0)
        {
            throw new ArgumentException("NodeConfigs must not be empty");
        }


        var convertedConfigs = new NodeConfigs<T>[configs.Length];
        for (var i = 0; i < configs.Length; i++)
        {
            convertedConfigs[i] = Of(configs[i]);
        }
        return Of(convertedConfigs, collectiveConditions, chained);
    }

    public static NodeConfigs<T> Of(NodeConfigs<T>[] configs, Condition[]? collectiveConditions = null, Chain chained = Chain.No)
    {
        if (configs == null || configs.Length == 0)
        {
            throw new ArgumentException("NodeConfigs must not be empty");
        }
        collectiveConditions ??= [];
        if (configs.Length == 1)
        {
            // Treat it as a single node and apply collective conditions to the node
            var node = configs[0];
            foreach (var condition in collectiveConditions)
            {
                node.DistributiveRunIf(condition);
            }
            return node;
        }
        return new Configs(configs, collectiveConditions, chained);
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

        public override NodeConfigs<T> DistributiveRunIf(Condition condition)
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

        public override NodeConfigs<T> RunIf(Condition condition)
        {
            Config.Conditions.Add(condition);
            return this;
        }

        public override NodeConfigs<T> SetChained()
        {
            //no-op
            return this;
        }

        public override NodeConfigs<T> SetChainedIgnoreDeferred()
        {
            //no-op
            return this;
        }
    }

    public class Configs : NodeConfigs<T>
    {
        /// <summary>
        /// Configurations for each element of the tuple
        /// </summary>
        public readonly NodeConfigs<T>[] NodeConfigs;
        /// <summary>
        /// Run conditions applied to everything in the tuple.
        /// </summary>
        public List<Condition> CollectiveConditions;
        /// <summary>
        /// See <see cref="Chained"/> for usage.
        /// </summary>
        public Chain Chained;

        public Configs(NodeConfigs<T>[] nodeConfigs, Condition[] collectiveConditions, Chain chained)
        {
            if (nodeConfigs == null || nodeConfigs.Length == 0)
            {
                throw new ArgumentException("NodeConfigs must not be empty");
            }
            NodeConfigs = nodeConfigs;
            CollectiveConditions = collectiveConditions.ToList();
            Chained = chained;
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

        public override NodeConfigs<T> DistributiveRunIf(Condition condition)
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

        public override NodeConfigs<T> RunIf(Condition condition)
        {
            foreach (var cfg in NodeConfigs)
            {
                cfg.RunIf(condition);
            }
            return this;
        }

        public override NodeConfigs<T> SetChained()
        {
            Chained = Chain.Yes;
            return this;
        }

        public override NodeConfigs<T> SetChainedIgnoreDeferred()
        {
            Chained = Chain.YesIgnoreDeferred;
            return this;
        }
    }

    /// <summary>
    /// Adds a system set to the systems
    /// </summary>
    /// <param name="set"></param>
    public abstract NodeConfigs<T> InSet(IIntoSystemSet set);

    /// <summary>
    /// Adds a system to the set represented by the enum
    /// </summary>
    /// <param name="set"></param>
    /// <typeparam name="TEnum"></typeparam>
    public NodeConfigs<T> InSet<TEnum>(TEnum set) where TEnum : struct, Enum
    {
        return InSet(new EnumSystemSet<TEnum>(set));
    }

    public abstract NodeConfigs<T> Before(IIntoSystemSet set);

    public abstract NodeConfigs<T> After(IIntoSystemSet set);

    public abstract NodeConfigs<T> BeforeIgnoreDeferred(IIntoSystemSet set);

    public abstract NodeConfigs<T> AfterIgnoreDeferred(IIntoSystemSet set);

    public abstract NodeConfigs<T> DistributiveRunIf(Condition condition);

    public abstract NodeConfigs<T> AmbiguousWith(IIntoSystemSet set);

    public abstract NodeConfigs<T> AmbiguousWithAll();

    public abstract NodeConfigs<T> RunIf(Condition condition);

    public abstract NodeConfigs<T> SetChained();

    public abstract NodeConfigs<T> SetChainedIgnoreDeferred();
}

public enum Chain
{
    /// <summary>
    /// Run nodes in order. If there are deferred parameters in preceding systems a ApplyDeferred will be added on the edge
    /// </summary>
    Yes,
    /// <summary>
    /// Run nodes in order. This will not add ApplyDeferred on the edge
    /// </summary>
    YesIgnoreDeferred,
    // Nodes are allowed to run in any order
    No
}
