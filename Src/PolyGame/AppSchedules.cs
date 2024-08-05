using System.Diagnostics.Contracts;
using PolyECS.Queries;
using PolyECS.Scheduling.Configs;
using PolyECS.Scheduling.Graph;
using PolyECS.Systems;

namespace PolyGame;

public partial class App
{
    public App AddSystems(IIntoScheduleLabel label, params IIntoNodeConfigs<RunSystem>[] systems)
    {
        var schedules = World.GetResource<ScheduleContainer>().TryGet().OrThrow(() => new ApplicationException("ScheduleContainer resource not found"));
        var schedule = schedules.Get(label.IntoScheduleLabel());
        if (schedule == null)
        {
            throw new ArgumentException($"Schedule for label {label} not found");
        }
        schedule.AddSystems(systems);
        return this;
    }
    
    public App AddSchedule(Schedule schedule)
    {
        var schedules = World.GetResource<ScheduleContainer>().TryGet().OrThrow(() => new ApplicationException("ScheduleContainer resource not found"));
        schedules.Insert(schedule);
        return this;
    }

    public App ConfigureSets(IIntoScheduleLabel label, params IIntoNodeConfigs<ISystemSet>[] sets)
    {
        var schedules = World.GetResource<ScheduleContainer>().TryGet().OrThrow(() => new ApplicationException("ScheduleContainer resource not found"));
        var schedule = schedules.Get(label.IntoScheduleLabel());
        if (schedule == null)
        {
            throw new ArgumentException($"Schedule for label {label} not found");
        }
        schedule.ConfigureSets(sets);
        return this;
    }

    public App ConfigureSchedules(ScheduleBuildSettings settings)
    {
        var schedules = World.GetResource<ScheduleContainer>().TryGet().OrThrow(() => new ApplicationException("ScheduleContainer resource not found"));
        schedules.SetBuildSettings(settings);
        return this;
    }
}
