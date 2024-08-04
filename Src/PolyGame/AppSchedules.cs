using PolyECS.Scheduling.Configs;
using PolyECS.Systems;

namespace PolyGame;

public partial class App
{
    public Schedule AddSystems(ScheduleLabel label, IIntoSystemConfigs systems)
    {
        return AddSystems(label, systems.IntoSystemConfig());
    }

    public Schedule AddSystems(ScheduleLabel label, NodeConfigs<RunSystem> systems)
    {
        var schedules = World.GetResource<ScheduleContainer>().TryGet().OrThrow(() => new ApplicationException("ScheduleContainer resource not found"));
        var schedule = schedules.Get(label);
        if (schedule == null)
        {
            throw new ArgumentException($"Schedule for label {label} not found");
        }
        schedule.AddSystems(systems);
        return schedule;
    }

    public Schedule AddSystems(IIntoSystemConfigs systems)
    {
        return AddSystems(Schedules.Default, systems);
    }

    public Schedule AddSystems(NodeConfigs<RunSystem> systems)
    {
        return AddSystems(Schedules.Default, systems);
    }


    public void AddSchedule(Schedule schedule)
    {
        var schedules = World.GetResource<ScheduleContainer>().TryGet().OrThrow(() => new ApplicationException("ScheduleContainer resource not found"));
        schedules.Insert(schedule);
    }
}
