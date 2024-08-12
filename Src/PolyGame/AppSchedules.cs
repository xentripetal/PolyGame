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

    public App AddSystem<T>(IIntoScheduleLabel label, Func<T, IIntoNodeConfigs<RunSystem>>? cfg = null) where T : IIntoNodeConfigs<RunSystem>, new()
    {
        if (cfg != null)
        {
            return AddSystems(label, cfg(new T()));
        }
        return AddSystems(label, new T());
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

    public App ConfigureSet<T>(IIntoScheduleLabel label, T set) where T : struct, Enum
    {
        return ConfigureSets(label, SetConfigs.Of(set));
    }

    public App ConfigureSchedules(ScheduleBuildSettings settings)
    {
        var schedules = World.GetResource<ScheduleContainer>().TryGet().OrThrow(() => new ApplicationException("ScheduleContainer resource not found"));
        schedules.SetBuildSettings(settings);
        return this;
    }
}
