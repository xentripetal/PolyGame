using System.Diagnostics.CodeAnalysis;
using DotNext;
using Flecs.NET.Core;
using PolyECS;

namespace PolyGame;

public partial class App
{
    public App Register<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                    DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    T>()
    {
        World.Register<T>();
        return this;
    }

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