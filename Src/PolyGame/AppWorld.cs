
using System.Diagnostics.CodeAnalysis;
using DotNext.Runtime;
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

    public Component<T> RegisterType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>()
    {
        return World.Register<T>();
    }
    
    public App RegisterResource<T>()
    {
        World.RegisterResource<T>();
        return this;
    }
    
    public App SetResource<T>(T resource)
    {
        World.SetResource(resource);
        return this;
    }
    
}
