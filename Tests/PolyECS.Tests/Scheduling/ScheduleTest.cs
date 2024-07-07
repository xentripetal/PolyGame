using System.Collections.Generic;
using Flecs.NET.Core;
using JetBrains.Annotations;
using PolyECS.Systems;
using PolyECS.Systems.Configs;

namespace PolyECS.Tests.Scheduling;

[TestSubject(typeof(Schedule))]
public class ScheduleTest
{
    protected class TestSystem : ClassSystem<object?>
    {
        public int InitCount = 0;
        public int RunCount = 0;

        public override void Initialize(PolyWorld world)
        {
            InitCount++;
        }

        public override void Run([CanBeNull] object param)
        {
            RunCount++;
        }

        public void AssertCalled()
        {
            Assert.Equal(1, InitCount);
            Assert.Equal(1, RunCount);
        }

        public TestSystem(string name) : base(new VoidParam(), name)
        {
            Meta.HasDeferred = true;
        }
    }

    [Fact]
    public void EmptySchedule()
    {
        var schedule = new Schedule();
        using var world = new PolyWorld();
        schedule.Run(world);
    }

    protected Schedule ScheduleAndRun(params NodeConfigs<RunSystem>[] configs)
    {
        var schedule = new Schedule();
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
        sysA.AssertCalled();
        sysB.AssertCalled();
    }

    [Fact]
    public void DoesntInsertASyncPoint()
    {
        var schedule = ScheduleAndRun(SystemConfigs.Of([new TestSystem("A"), new TestSystem("B")], chained: Chain.No));
        Assert.Equal(2, schedule.Executable.Systems.Count);
    }
}
