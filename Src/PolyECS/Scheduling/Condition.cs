namespace PolyECS.Scheduling;

public interface Condition
{
    public void Initialize(PolyWorld world);
    public bool Evaluate(PolyWorld world);
}
