using Flecs.NET.Core;

namespace PolyECS.Systems;

public interface ISystemParam
{
    public void Initialize(World world);
    public void EvaluateNewTable(SystemMeta meta, Table table);
}
