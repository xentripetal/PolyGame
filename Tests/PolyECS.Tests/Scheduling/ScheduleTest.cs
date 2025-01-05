using JetBrains.Annotations;
using PolyECS.Scheduling;
using PolyECS.Scheduling.Configs;
using PolyECS.Systems;

namespace PolyECS.Tests.Scheduling;

[TestSubject(typeof(Schedule))]
public class ScheduleTest
{
    [Fact]
    public void EmptySchedule()
    {
        var schedule = new Schedule(new ScheduleLabel("default"));
        using var world = new PolyWorld();
        schedule.Run(world);
    }

    protected Schedule ScheduleAndRun(params IIntoNodeConfigs<ISystem>[] configs)
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
        var sysA = new TestSystem();
        var sysB = new TestSystem();
        var schedule = ScheduleAndRun(SystemConfigs.Of([sysA, sysB]).Chained());

        // Should have our 2 systems and a sync point between them
        Assert.Equal(3, schedule.Executable.Systems.Count);
        sysA.AssertCalled(1);
        sysB.AssertCalled(1);
    }

    [Fact]
    public void DoesntInsertASyncPoint()
    {
        var schedule = ScheduleAndRun(new TestSystem(), new TestSystem());
        Assert.Equal(2, schedule.Executable.Systems.Count);
    }

    protected class TestSystem : ClassSystem
    {
        public int RunCount;

        public TestSystem() => Meta.HasDeferred = true;


        public void AssertCalled(int times)
        {
            Assert.Equal(times, RunCount);
        }

        protected override void BuildParameters(ParamBuilder builder)
        {
        }

        public override void Run(PolyWorld world)
        {
            RunCount++;
        }
    }
}
