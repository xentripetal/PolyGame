namespace PolyECS;

using static Tracy.PInvoke;

public interface IProfilerZone : IDisposable
{
    uint Id { get; }

    int Active { get; }

    void EmitName(string name);

    void EmitColor(uint color);

    void EmitText(string text);
}

public readonly struct NoopProfilerZone : IProfilerZone
{
    public void Dispose()
    { }

    public uint Id => 0;

    public int Active => 0;

    public void EmitName(string name)
    { }

    public void EmitColor(uint color)
    { }

    public void EmitText(string text)
    { }
}

public readonly struct ProfilerZone : IProfilerZone
{
    public readonly TracyCZoneCtx Context;

    public uint Id => Context.Data.Id;

    public int Active => Context.Data.Active;

    internal ProfilerZone(TracyCZoneCtx context)
    {
        Context = context;
    }

    public void EmitName(string name)
    {
        using var namestr = Profiler.GetCString(name, out var nameln);
        TracyEmitZoneName(Context, namestr, nameln);
    }

    public void EmitColor(uint color)
    {
        TracyEmitZoneColor(Context, color);
    }

    public void EmitText(string text)
    {
        using var textstr = Profiler.GetCString(text, out var textln);
        TracyEmitZoneText(Context, textstr, textln);
    }

    public void Dispose()
    {
        TracyEmitZoneEnd(Context);
    }
}