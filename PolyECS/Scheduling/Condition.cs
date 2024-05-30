namespace PolyFlecs.Systems;

public interface Condition
{
    public void Initialize(World world);
    public bool Evaluate(World world);
}
