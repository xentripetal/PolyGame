using System.Collections.Generic;
using Flecs.NET.Core;
using JetBrains.Annotations;
using PolyECS.Scheduling.Configs;
using PolyECS.Systems;

namespace PolyECS.Tests.Scheduling;

[TestSubject(typeof(Schedule))]
public class ScheduleTest
{
    protected class TestSystem : ClassSystem<object?>
    {
        public int InitCout = 0;
        public int RunCount = 0;

        protected override ISystemParam<object> CreateParam(PolyWorld world) => new VoidParam();

        public override void Run([CanBeNull] object param)
        {
            RunCount++;
        }

        public void AssertCalled(int times)
        {
            Assert.Equal(times, RunCount);
        }

        public TestSystem(string name) : base(name)
        {
            Meta.HasDeferred = true;
        }
    }

    [Fact]
    public void EmptySchedule()
    {
        var schedule = new Schedule(new ScheduleLabel("default"));
        using var world = new PolyWorld();
        schedule.Run(world);
    }

    protected Schedule ScheduleAndRun(params NodeConfigs<RunSystem>[] configs)
    {
        var schedule = new Schedule(new ScheduleLabel("default"));
        using var world = new PolyWorld();
        schedule.AddSystems(configs);
        schedule.Run(world);
        return schedule;
    }

    [Fact]
    public void InsertsASyncPoint()
    {
        var sysA = new TestSystem("A");
        var sysB = new TestSystem("B");
        var schedule = ScheduleAndRun(SystemConfigs.Of([sysA, sysB], chained: Chain.Yes));

        // Should have our 2 systems and a sync point between them
        Assert.Equal(3, schedule.Executable.Systems.Count);
        sysA.AssertCalled(1);
        sysB.AssertCalled(1);
    }

    [Fact]
    public void DoesntInsertASyncPoint()
    {
        var schedule = ScheduleAndRun(SystemConfigs.Of([new TestSystem("A"), new TestSystem("B")], chained: Chain.No));
        Assert.Equal(2, schedule.Executable.Systems.Count);
    }
}
