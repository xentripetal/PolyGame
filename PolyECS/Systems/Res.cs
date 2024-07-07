using Flecs.NET.Core;

namespace PolyECS.Systems;

public class Res<T>
{
    public readonly T Value;

    public static implicit operator T(Res<T> res) => res.Value;
}

public class ResMut<T>
{
}
