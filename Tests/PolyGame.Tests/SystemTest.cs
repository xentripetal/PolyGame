using System;
using PolyECS;
using PolyECS.Scheduling;

namespace PolyGame.Tests;

public abstract class SystemTest : IDisposable
{
    protected Schedule Schedule;

    protected ScheduleLabel ScheduleLabel = new ("TestSchedule");
    protected PolyWorld World;

    public SystemTest()
    {
        World = new PolyWorld();
        var schedules = World.GetResource<ScheduleContainer>().TryGet().OrThrow(() => new ApplicationException("ScheduleContainer resource not found"));
        Schedule = new Schedule(ScheduleLabel);
        schedules.Insert(Schedule);
    }

    public void Dispose()
    {
        World.Dispose();
    }

    public void Progress()
    {
        World.World.Progress();
        World.RunSchedule(ScheduleLabel);
    }
}
