using PolyECS.Systems;

namespace PolyGame;

public static class Schedules
{
    public static readonly ScheduleLabel PreStartup = new ("pre-startup");
    public static readonly ScheduleLabel Startup = new ("startup");
    public static readonly ScheduleLabel PostStartup = new ("post-startup");

    public static readonly ScheduleLabel FirstUpdate = new ("first-update");
    public static readonly ScheduleLabel PreUpdate = new ("pre-update");
    public static readonly ScheduleLabel Update = new ("update");
    public static readonly ScheduleLabel PostUpdate = new ("post-update");
    public static readonly ScheduleLabel LastUpdate = new ("last-update");

    public static readonly ScheduleLabel FirstRender = new ("first-render");
    public static readonly ScheduleLabel PreRender = new ("pre-render");
    public static readonly ScheduleLabel Render = new ("render");
    public static readonly ScheduleLabel PostRender = new ("post-render");
    public static readonly ScheduleLabel LastRender = new ("last-render");
    
    public static ScheduleLabel Default = Update;
}

public class MainScheduleOrder
{
    public List<ScheduleLabel> UpdateLabels = new ([
        Schedules.FirstUpdate,
        Schedules.PreUpdate,
        Schedules.Update,
        Schedules.PostUpdate,
        Schedules.LastUpdate
    ]);

    public List<ScheduleLabel> RenderLabels = new ([
        Schedules.FirstRender,
        Schedules.PreRender,
        Schedules.Render,
        Schedules.PostRender,
        Schedules.LastRender
    ]);

    public List<ScheduleLabel> StartupLabels = new ([
        Schedules.PreStartup,
        Schedules.Startup,
        Schedules.PostStartup,
    ]);

    public void InsertUpdateAfter(ScheduleLabel after, ScheduleLabel label)
    {
        var index = UpdateLabels.IndexOf(after);
        if (index == -1)
        {
            throw new ArgumentException($"Label {after} not found in schedule order");
        }
        UpdateLabels.Insert(index + 1, label);
    }

    public void InsertUpdateBefore(ScheduleLabel before, ScheduleLabel label)
    {
        var index = UpdateLabels.IndexOf(before);
        if (index == -1)
        {
            throw new ArgumentException($"Label {before} not found in schedule order");
        }
        UpdateLabels.Insert(index, label);
    }

    public void InsertStartupAfter(ScheduleLabel after, ScheduleLabel label)
    {
        var index = StartupLabels.IndexOf(after);
        if (index == -1)
        {
            throw new ArgumentException($"Label {after} not found in schedule order");
        }
        StartupLabels.Insert(index + 1, label);
    }

    public void InsertStartupBefore(ScheduleLabel before, ScheduleLabel label)
    {
        var index = StartupLabels.IndexOf(before);
        if (index == -1)
        {
            throw new ArgumentException($"Label {before} not found in schedule order");
        }
        StartupLabels.Insert(index, label);
    }

    public void InsertRenderAfter(ScheduleLabel after, ScheduleLabel label)
    {
        var index = RenderLabels.IndexOf(after);
        if (index == -1)
        {
            throw new ArgumentException($"Label {after} not found in schedule order");
        }
        RenderLabels.Insert(index + 1, label);
    }

    public void InsertRenderBefore(ScheduleLabel before, ScheduleLabel label)
    {
        var index = RenderLabels.IndexOf(before);
        if (index == -1)
        {
            throw new ArgumentException($"Label {before} not found in schedule order");
        }
        RenderLabels.Insert(index, label);
    }

    public void RemoveUpdate(ScheduleLabel label)
    {
        UpdateLabels.Remove(label);
    }

    public void RemoveStartup(ScheduleLabel label)
    {
        StartupLabels.Remove(label);
    }

    public void RemoveRender(ScheduleLabel label)
    {
        RenderLabels.Remove(label);
    }
}
