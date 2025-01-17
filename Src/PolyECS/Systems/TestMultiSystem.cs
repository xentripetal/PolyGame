using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Flecs.NET.Core;

namespace PolyECS.Systems;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                            DynamicallyAccessedMemberTypes.PublicParameterlessConstructor |
                            DynamicallyAccessedMemberTypes.PublicNestedTypes |
                            DynamicallyAccessedMemberTypes.PublicMethods)]
public abstract class TestMultiSystem
{
    public void Initialize()
    {
        var t = GetType();
        var methods = t.GetMethods();
        foreach (var method in methods)
        {
            foreach (var p in method.GetParameters())
            {
                var pt = p.GetType();
                // TODO can we pass attribute down to method parameters?
                pt.GetConstructor(Type.EmptyTypes);
            }
        }
    }
}