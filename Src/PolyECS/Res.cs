using DotNext;
using Flecs.NET.Core;
using PolyECS.Systems;

namespace PolyECS;

// TODO refactor Res to actually hold the value instead of looking it up every time

public class Res<T>
{
    protected PolyWorld World;
    protected ulong ResourceId;

    public Res(PolyWorld world)
    {
        World = world;
        ResourceId = world.RegisterResource<T>();
    }
    
    public bool IsEmpty => !HasValue;
    public bool HasValue => World.Resources.HasValue(ResourceId);

    public T? Get()
    {
        World.Resources.TryGet(ResourceId, out T? value);
        return value;
    }

    public T? Value => Get();

    public static implicit operator T?(Res<T> res) => res.Get();
}

public class ResMut<T> : Res<T>
{
    public ResMut(PolyWorld world) : base(world) { }
    
    public void Set(T? value)
    {
        World.Resources.Set(ResourceId, value);
    }
}
