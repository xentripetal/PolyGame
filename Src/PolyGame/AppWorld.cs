using System.Diagnostics.CodeAnalysis;
using DotNext;
using Flecs.NET.Core;
using PolyECS;

namespace PolyGame;

public partial class App
{
    public App RegisterComponent<T>() where T : IComponent
    {
        World.RegisterComponent<T>();
        return this;
    }

    public Component<T> RegisterType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>() => World.Register<T>();

    public App RegisterResource<T>()
    {
        World.RegisterResource<T>();
        return this;
    }
    
    public Res<T> GetResource<T>() => World.GetResource<T>();

    public App SetResource<T>(T resource)
    {
        World.SetResource(resource);
        return this;
    }
}
