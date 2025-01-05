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
        var schedules = World.MustGetResource<ScheduleContainer>();
        Schedule = new Schedule(ScheduleLabel);
        schedules.Insert(Schedule);
    }

    public void Dispose()
    {
        World.Dispose();
    }

    public void Progress()
    {
        World.FlecsWorld.Progress();
        World.RunSchedule(ScheduleLabel);
    }
}
