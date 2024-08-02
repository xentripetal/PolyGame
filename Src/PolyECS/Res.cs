using Flecs.NET.Core;
using PolyECS.Systems;

namespace PolyECS;

public class Res<T> : IIntoSystemParam<Res<T>>
{
    public T Get()
    {
        return World.Get<T>();
    }

    public Res(World world)
    {
        World = world;
    }

    protected World World;

    public bool IsEmpty => !World.Has<T>();
    public bool HasValue => World.Has<T>();

    public static implicit operator T(Res<T> res) => res.Get();
    public static ISystemParam<Res<T>> IntoParam(PolyWorld world) => new ResParam<T>();
}

public class ResMut<T> : Res<T>, IIntoSystemParam<ResMut<T>>
{
    public ResMut(World world) : base(world) { }

    public Ref<T> GetRef()
    {
        return World.GetRef<T>();
    }

    public static implicit operator T(ResMut<T> res) => res.Get();

    public ref T Get()
    {
        return ref World.GetRef<T>().Get();
    }

    public static ISystemParam<ResMut<T>> IntoParam(PolyWorld world) => new ResMutParam<T>();
}
