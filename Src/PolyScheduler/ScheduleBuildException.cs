namespace PolyScheduler;

public class ScheduleBuildException : ArgumentException
{
    public ScheduleBuildException(string message) : base(message) { }

    public class Ambiguity : ScheduleBuildException
    {
        public Ambiguity(string message) : base(message) { }
    }

    public class CrossDependency : ScheduleBuildException
    {
        public CrossDependency(string a, string b) : base($"Cross dependency between {a} and {b}") { }
    }

    public class SetsHaveOrderButIntersect : ScheduleBuildException
    {
        public SetsHaveOrderButIntersect(string a, string b) : base(
            $"{a} and {b} have a `before`-`after` relationship (which may be transitive) but share systems.") { }
    }

    public class SystemTypeSetAmbiguity : ScheduleBuildException
    {
        public SystemTypeSetAmbiguity(string a) : base(
            $"Tried to order against `{a}` in a schedule that has more than one `{a}` instance. `{a}` is a `SystemTypeSet` and cannot be used for ordering if ambiguous. Use a different set without this restriction.") { }
    }

    public class HierarchyLoop : ScheduleBuildException
    {
        public HierarchyLoop(string a) : base($"System set `{a}` contains itself.") { }
    }

    public class HierarchyCycle : ScheduleBuildException
    {
        public HierarchyCycle(string a) : base($"System set `{a}` contains a cycle.") { }
    }

    public class HierarchyRedundancy : ScheduleBuildException
    {
        public HierarchyRedundancy(string message) : base($"System set hierarchy contains redundant edges.\n{message}") { }
    }

    public class DependencyLoop : ScheduleBuildException
    {
        public DependencyLoop(string message) : base($"System set `{message}` depends on itself.") { }
    }

    public class DependencyCycle : ScheduleBuildException
    {
        public DependencyCycle(string message) : base($"System dependencies contain cycle(s).\n{message}") { }
    }

    public class Uninitialized : ScheduleBuildException
    {
        public Uninitialized() : base("Systems in schedule have not been initialized.") { }
    }
}
