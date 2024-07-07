using System.Dynamic;

namespace PolyECS.Systems;

public struct Tick
{
    private uint Value;

    public uint Get() => Value;
}
