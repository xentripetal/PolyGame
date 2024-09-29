using DotNext;
using Flecs.NET.Core;
using PolyECS.Systems;

namespace PolyECS;

// TODO refactor Res to actually hold the value instead of looking it up every time

public class Res<T> : IIntoSystemParam<Res<T>>
{
    protected World World;

    public Res(World world) => World = world;

    public bool IsEmpty => !World.Has<T>();
    public bool HasValue => World.Has<T>();
    public static ITSystemParam<Res<T>> IntoParam(PolyWorld world) => new ResParam<T>();

    public T? Get()
    {
        // TODO - Move Res out of World and into its own storage system
        if (!World.Has<T>())
        {
            return default;
        }
        return World.Get<T>();
    }

    public T? Value => Get();

    public Optional<T> TryGet()
    {
        if (World.Has<T>())
        {
            return World.Get<T>();
        }
        return Optional<T>.None;
    }

    public static implicit operator T?(Res<T> res) => res.Get();
}

public class ResMut<T> : Res<T>, IIntoSystemParam<ResMut<T>>
{
    public ResMut(World world) : base(world) { }

    public new static ITSystemParam<ResMut<T>> IntoParam(PolyWorld world) => new ResMutParam<T>();

    public Ref<T> GetRef() => World.GetRef<T>();
    
    public static implicit operator T(ResMut<T> res) => res.Get();
}
