namespace PolyECS.Systems;

public interface Condition
{
    public void Initialize(IScheduleWorld world);
    public bool Evaluate(IScheduleWorld world);
}
