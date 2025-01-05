using Flecs.NET.Core;
using PolyECS.Scheduling.Graph;

namespace PolyECS.Scheduling;

/// <summary>
///     Resource that stores <see cref="Schedule" />s mapped to <see cref="ScheduleLabel" />s excluding the current running
///     <see cref="Schedule" />
/// </summary>
/// <remarks>Based on bevy_ecs::schedule:Schedules</remarks>
public class ScheduleContainer
{
    public HashSet<ulong> IgnoredSchedulingAmbiguities = new ();
    protected Dictionary<ScheduleLabel, Schedule> Schedules = new ();

    /// <summary>
    ///     Inserts a labeled schedule into the map. Replaces existing schedule if label already exists.
    /// </summary>
    /// <param name="schedule">
    ///     If the map already had an entry for the label, this is the old schedule that was replaced. Else
    ///     its null.
    /// </param>
    /// <returns></returns>
    public Schedule? Insert(Schedule schedule)
    {
        if (Schedules.ContainsKey(schedule.GetLabel()))
        {
            var existing = Schedules[schedule.GetLabel()];
            Schedules[schedule.GetLabel()] = schedule;
            return existing;
        }
        Schedules[schedule.GetLabel()] = schedule;
        return null;
    }

    public Schedule? Remove(ScheduleLabel label)
    {
        if (Schedules.ContainsKey(label))
        {
            var schedule = Schedules[label];
            Schedules.Remove(label);
            return schedule;
        }
        return null;
    }

    public bool Contains(ScheduleLabel label) => Schedules.ContainsKey(label);

    public Schedule? Get(ScheduleLabel label)
    {
        Schedules.TryGetValue(label, out var schedule);
        return schedule;
    }

    public IEnumerable<Schedule> GetAll() => Schedules.Values;

    public void SetBuildSettings(ScheduleBuildSettings settings)
    {
        foreach (var schedule in Schedules.Values)
        {
            schedule.SetBuildSettings(settings);
        }
    }

    public void AllowAmbiguousComponent(ulong componentId)
    {
        IgnoredSchedulingAmbiguities.Add(componentId);
    }

    public void AllowAmbiguousComponent<T>(PolyWorld world)
    {
        unsafe
        {
            var id = Type<T>.Id(world.FlecsWorld.Handle);
            AllowAmbiguousComponent(id);
        }
    }
}
