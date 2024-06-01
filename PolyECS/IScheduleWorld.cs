namespace PolyECS;

public interface IScheduleWorld : IDeferredScheduleWorld
{
    public IDeferredScheduleWorld AsDeferred();
    public void BeforeRun();
    public void AfterRun();
}