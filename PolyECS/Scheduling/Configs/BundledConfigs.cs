using PolyECS.Scheduling.Graph;
using PolyFlecs.Systems.Graph;

namespace PolyFlecs.Systems.Configs;


/// <summary>
/// A collection of generic <see cref="NodeConfig{T}"/>s
///
/// A port of bevy_ecs::schedule::config::NodeConfigs
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class NodeConfigs<T>
{
    public static NodeConfigs<System> NewSystem(System system)
    {
        var sets = system.GetDefaultSystemSets();
        return new NodeConfigs<System>.Node(new SystemConfig
        {
            Node = system,
            Subgraph = new SubgraphInfo
            {
                Hierarchy = sets
            }
        });
    }

    public class Node(NodeConfig<T> config) : NodeConfigs<T>
    {
        public NodeConfig<T> Config = config;

        public override NodeConfigs<T> InSet(SystemSet set)
        {
            Config.Subgraph.Hierarchy.Add(set);
            return this;
        }

        public override NodeConfigs<T> Before(SystemSet set)
        {
            Config.Subgraph.Dependencies.Add(new Dependency(DependencyKind.Before, set));
            return this;
        }

        public override NodeConfigs<T> After(SystemSet set)
        {
            Config.Subgraph.Dependencies.Add(new Dependency(DependencyKind.After, set));
            return this;
        }

        public override NodeConfigs<T> BeforeIgnoreDeferred(SystemSet set)
        {
            Config.Subgraph.Dependencies.Add(new Dependency(DependencyKind.BeforeNoSync, set));
            return this;
        }

        public override NodeConfigs<T> AfterIgnoreDeferred(SystemSet set)
        {
            Config.Subgraph.Dependencies.Add(new Dependency(DependencyKind.AfterNoSync, set));
            return this;
        }

        public override NodeConfigs<T> DistributiveRunIf(Condition condition)
        {
            Config.Conditions.Add(condition);
            return this;
        }

        public override NodeConfigs<T> AmbiguousWith(SystemSet set)
        {
            Config.Subgraph.AddAmbiguousWith(set);
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
        public List<NodeConfigs<T>> NodeConfigs;
        /// <summary>
        /// Run conditions applied to everything in the tuple.
        /// </summary>
        public List<Condition> CollectiveConditions;
        /// <summary>
        /// See <see cref="Chained"/> for usage.
        /// </summary>
        public Chain Chained;

        public Configs(List<NodeConfigs<T>> nodeConfigs, List<Condition> collectiveConditions, Chain chained)
        {
            NodeConfigs = nodeConfigs;
            CollectiveConditions = collectiveConditions;
            Chained = chained;
        }

        public override NodeConfigs<T> InSet(SystemSet set)
        {
            foreach (var cfg in NodeConfigs)
            {
                cfg.InSet(set);
            }
            return this;
        }

        public override NodeConfigs<T> Before(SystemSet set)
        {
            foreach (var cfg in NodeConfigs)
            {
                cfg.Before(set);
            }
            return this;
        }

        public override NodeConfigs<T> After(SystemSet set)
        {
            foreach (var cfg in NodeConfigs)
            {
                cfg.After(set);
            }
            return this;
        }

        public override NodeConfigs<T> BeforeIgnoreDeferred(SystemSet set)
        {
            foreach (var cfg in NodeConfigs)
            {
                cfg.BeforeIgnoreDeferred(set);
            }
            return this;
        }

        public override NodeConfigs<T> AfterIgnoreDeferred(SystemSet set)
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

        public override NodeConfigs<T> AmbiguousWith(SystemSet set)
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
    public abstract NodeConfigs<T> InSet(SystemSet set);

    public abstract NodeConfigs<T> Before(SystemSet set);

    public abstract NodeConfigs<T> After(SystemSet set);

    public abstract NodeConfigs<T> BeforeIgnoreDeferred(SystemSet set);

    public abstract NodeConfigs<T> AfterIgnoreDeferred(SystemSet set);

    public abstract NodeConfigs<T> DistributiveRunIf(Condition condition);

    public abstract NodeConfigs<T> AmbiguousWith(SystemSet set);

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
