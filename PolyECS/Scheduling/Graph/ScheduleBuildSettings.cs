namespace PolyECS.Scheduling.Graph;

public struct ScheduleBuildSettings(
    bool throwAmbiguousErrors = false,
    bool throwHierarchyRedundancyErrors = false,
    bool autoInsertApplyDeferred = true,
    bool useShortNames = true,
    bool reportSets = true)
{
    /// Determines whether the presence of ambiguities (systems with conflicting access but indeterminate order)
    /// is only logged or also results in an [`Ambiguity`](ScheduleBuildError::Ambiguity) error.
    public readonly bool ThrowAmbiguousErrors = throwAmbiguousErrors;
    /// Determines whether the presence of redundant edges in the hierarchy of system sets is only
    /// logged or also results in a [`HierarchyRedundancy`](ScheduleBuildError::HierarchyRedundancy)
    /// error.
    public readonly bool ThrowHierarchyRedundancyErrors = throwHierarchyRedundancyErrors;
    /// Auto insert [`apply_deferred`] systems into the schedule,
    /// when there are [`Deferred`](crate::prelude::Deferred)
    /// in one system and there are ordering dependencies on that system. [`Commands`](crate::system::Commands) is one
    /// such deferred buffer.
    ///
    /// You may want to disable this if you only want to sync deferred params at the end of the schedule,
    /// or want to manually insert all your sync points.
    ///
    /// Defaults to `true`
    public readonly bool AutoInsertApplyDeferred = autoInsertApplyDeferred;
    /// If set to true, node names will be shortened instead of the fully qualified type path.
    ///
    /// Defaults to `true`.
    public readonly bool UseShortNames = useShortNames;
    /// If set to true, report all system sets the conflicting systems are part of.
    ///
    /// Defaults to `true`.
    public readonly bool ReportSets = reportSets;
}
