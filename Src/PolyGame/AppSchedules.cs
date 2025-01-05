using PolyECS;
using PolyECS.Scheduling;
using PolyECS.Scheduling.Configs;
using PolyECS.Scheduling.Graph;
using PolyECS.Systems;

namespace PolyGame;

public partial class App
{
    public App AddSystems(IIntoScheduleLabel label, params IIntoNodeConfigs<ISystem>[] systems)
    {
        var schedules = World.MustGetResource<ScheduleContainer>();
        var schedule = schedules.Get(label.IntoScheduleLabel());
        if (schedule == null)
        {
            throw new ArgumentException($"Schedule for label {label} not found");
        }
        schedule.AddSystems(systems);
        return this;
    }
    
    public App AddSystem<T>(IIntoScheduleLabel label, Func<T, IIntoNodeConfigs<ISystem>>? cfg = null) where T : ClassSystem, new()
    {
        var sys = new T();
        
        if (cfg != null)
        {
            return AddSystems(label, cfg(sys));
        }

        return AddSystems(label, sys);
    }


    public App AddSchedule(Schedule schedule)
    {
        var schedules = World.MustGetResource<ScheduleContainer>();
        schedules.Insert(schedule);
        return this;
    }

    public App ConfigureSets(IIntoScheduleLabel label, params IIntoNodeConfigs<ISystemSet>[] sets)
    {
        var schedules = World.MustGetResource<ScheduleContainer>();
        var schedule = schedules.Get(label.IntoScheduleLabel());
        if (schedule == null)
        {
            throw new ArgumentException($"Schedule for label {label} not found");
        }
        schedule.ConfigureSets(sets);
        return this;
    }

    public App ConfigureSet<T>(IIntoScheduleLabel label, T set) where T : struct, Enum => ConfigureSets(label, SetConfigs.Of(set));

    public App ConfigureSchedules(ScheduleBuildSettings settings)
    {
        var schedules = World.MustGetResource<ScheduleContainer>();
        schedules.SetBuildSettings(settings);
        return this;
    }
}
