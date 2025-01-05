using DotNext;
using Flecs.NET.Core;
using PolyECS.Systems;

namespace PolyECS;

public struct Res<T> : IIntoSystemParam, IStaticSystemParam<Res<T>>
{
    private PolyWorld World;
    private int Index;

    public Res(PolyWorld world)
    {
        World = world;
        Index = world.RegisterResource<T>();
    }

    public bool IsEmpty => !HasValue;
    public bool HasValue => World.Resources.HasValue(Index);

    public T? Get()
    {
        World.Resources.TryGet<T>(Index, out var value);
        return value;
    }
    
    public bool TryGet(out T? value)
    {
        return World.Resources.TryGet<T>(Index, out value);
    }

    public T? Value => Get();

    public static implicit operator T?(Res<T> res) => res.Get();
    public ISystemParam IntoParam(PolyWorld world) => new ResParam<T>();
    public static Res<T> BuildParamValue(PolyWorld world)
    {
        return world.GetResource<T>();
    }

    public static ISystemParam GetParam(PolyWorld world, Res<T> value)
    {
        return value.IntoParam(world);
    }
}



public struct ResMut<T> : IIntoSystemParam, IStaticSystemParam<ResMut<T>>
{
    private PolyWorld World;
    private int Index;

    public ResMut(PolyWorld world)
    {
        World = world;
        Index = world.RegisterResource<T>();
    }
    
    public bool IsEmpty => !HasValue;
    public bool HasValue => World.Resources.HasValue(Index);



    public void Set(T value)
    {
        World.Resources.Set(Index, value);
    }
    
    public T Get()
    {
        World.Resources.TryGet<T>(Index, out var value);
        return value!;
    }
    
    public bool TryGet(out T? value)
    {
        return World.Resources.TryGet<T>(Index, out value);
    }

    public T Value
    {
        get => Get();
        set => Set(value);
    }

    public static implicit operator T(ResMut<T> res) => res.Get();
    
    public ISystemParam IntoParam(PolyWorld world) => new ResMutParam<T>();
    public static ResMut<T> BuildParamValue(PolyWorld world)
    {
        return world.GetResourceMut<T>();
    }

    public static ISystemParam GetParam(PolyWorld world, ResMut<T> value)
    {
        return value.IntoParam(world);
    }
}
