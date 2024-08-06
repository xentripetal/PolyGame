using DotNext;
using Flecs.NET.Core;
using PolyECS.Queries;
using PolyECS.Systems;

namespace PolyECS;

public class Res<T> : IIntoSystemParam<Res<T>>
{
    public T Get()
    {
        // TODO - Move Res out of World and into its own storage system
        return World.Get<T>();
    }
    
    public Optional<T> TryGet()
    {
        if (World.Has<T>())
        {
            return World.Get<T>();
        }
        return Optional<T>.None;
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

    public new ref T Get()
    {
        return ref World.GetRef<T>().Get();
    }

    public new static ISystemParam<ResMut<T>> IntoParam(PolyWorld world) => new ResMutParam<T>();
}
