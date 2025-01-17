using System.Reflection;
using Flecs.NET.Core;
using TypeInfo = Flecs.NET.Core.TypeInfo;

namespace PolyECS;

/// <summary>
/// When specified on a component that is registered with <see cref="PolyWorld.Register{T}"/>, it will use the provided
/// name for the component instead of the generated type name.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum)]
public class NamedComponentAttribute : Attribute
{
    public readonly string Name;

    public NamedComponentAttribute(string name)
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum,
    AllowMultiple = true)]
public abstract class ComponentRegistrationAttribute : Attribute
{
    public abstract void Register(PolyWorld world, UntypedComponent component);
}

/// <summary>
/// Adds a dependency on <see cref="T"/> to the component. When this component is added to an entity, the entity will
/// create the dependency if it does not already exist.
/// </summary>
/// <typeparam name="T"></typeparam>
public class RequiredComponentAttribute<T> : ComponentRegistrationAttribute
{
    public override unsafe void Register(PolyWorld world, UntypedComponent component)
    {
        component.Add(Ecs.With, Type<T>.Id(component.World));
    }
}

[AttributeUsage(AttributeTargets.Method)]
public abstract class UntypedMethodComponentRegistrationAttribute : Attribute
{
    public abstract void Apply(MethodInfo method, UntypedComponent component, PolyWorld world);
}

public class ComponentRegistrationParameterResolver
{
    protected Dictionary<Type, object> Values = new();

    public void AddParameter<T>(T value)
    {
        Values[typeof(T)] = value;
    }

    public void Invoke(MethodInfo info)
    {
        var parameters = info.GetParameters();
        var args = new object[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            if (Values.TryGetValue(parameters[i].ParameterType, out var value))
                args[i] = value;
            else
                throw new InvalidOperationException($"No value for parameter {parameters[i].Name}");
        }

        info.Invoke(null, args);
    }
}

[AttributeUsage(AttributeTargets.Method)]
public abstract class MethodComponentRegistrationAttribute<T> : Attribute
{
    public abstract void Apply(MethodInfo method, Component<T> component, PolyWorld world);
}

/// <summary>
/// Marks a method as defining a custom constructor for a component. The method must be static and take a ref T and optionally a <see cref="TypeInfo"/>.
/// By default, components will use the public parameterless constructor if it exists, this will override that.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ComponentCtorAttribute<T> : MethodComponentRegistrationAttribute<T>
{
    private delegate void MinimalCtorCallback<T>(ref T data);

    /// <summary>
    /// Tries to invoke 
    /// </summary>
    /// <param name="method">The method containing the attribute. Must be static and take a ref T param. </param>
    /// <param name="component">The component to register on to</param>
    /// <typeparam name="T"></typeparam>
    public override void Apply(MethodInfo method, Component<T> component, PolyWorld world)
    {
        if (!method.IsStatic)
        {
            throw new InvalidOperationException("Method must be static to be used as a ComponentCtor");
        }

        var parameters = method.GetParameters();
        if (parameters.Length == 2 && parameters[0].ParameterType == typeof(T).MakeByRefType() &&
            parameters[1].ParameterType == typeof(TypeInfo))
            component.Ctor(method.CreateDelegate<Ecs.CtorCallback<T>>());
        else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(T).MakeByRefType())
        {
            var call = method.CreateDelegate<MinimalCtorCallback<T>>();
            component.Ctor((ref T data, TypeInfo _) => call(ref data));
        }
        else
            throw new InvalidOperationException("Method must take a ref T and TypeInfo or just a ref T");
    }
}

/// <summary>
/// Helper for component registration attributes that automatically resolves parameters and invokes the method.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class AutoMethodComponentRegistrationAttribute<T> : MethodComponentRegistrationAttribute<T>
{
    public override void Apply(MethodInfo method, Component<T> component, PolyWorld world)
    {
        if (!method.IsStatic)
            throw new InvalidOperationException("Method must be static to be used as a ComponentMembers");
        var resolver = new ComponentRegistrationParameterResolver();
        resolver.AddParameter(component);
        resolver.AddParameter(component.UntypedComponent);
        resolver.AddParameter(world);
        resolver.AddParameter(world.FlecsWorld);
        BeforeApply(resolver, method, component, world);
        resolver.Invoke(method);
        AfterApply(method, component, world);
    }

    public virtual void BeforeApply(ComponentRegistrationParameterResolver resolver, MethodInfo method,
        Component<T> component, PolyWorld world)
    { }

    public virtual void AfterApply(MethodInfo method, Component<T> component, PolyWorld world)
    { }
}

/// <summary>
/// Helper for component registration attributes that automatically resolves parameters and invokes the method.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class AutoUntypedMethodComponentRegistrationAttribute : UntypedMethodComponentRegistrationAttribute
{
    public override void Apply(MethodInfo method, UntypedComponent component, PolyWorld world)
    {
        if (!method.IsStatic)
            throw new InvalidOperationException("Method must be static to be used as a ComponentMembers");
        var resolver = new ComponentRegistrationParameterResolver();
        resolver.AddParameter(component);
        resolver.AddParameter(world);
        resolver.AddParameter(world.FlecsWorld);
        BeforeApply(resolver, method, component, world);
        resolver.Invoke(method);
        AfterApply(method, component, world);
    }

    public virtual void BeforeApply(ComponentRegistrationParameterResolver resolver, MethodInfo method,
        UntypedComponent component, PolyWorld world)
    { }

    public virtual void AfterApply(MethodInfo method, UntypedComponent component, PolyWorld world)
    { }
}

/// <summary>
/// Provides a mechanism for registering a static method to define or modify the behavior
/// of a component's members at the time of its registration. This attribute allows users
/// to annotate a static method that is invoked with the component's metadata upon registration.
/// </summary>
/// <typeparam name="T">
/// The type of the component whose members are being defined or altered by the method.
/// </typeparam>
public class ComponentMembersAttribute<T> : RegistrationCallbackAttribute<T>
{ }

/// <inheritdoc cref="ComponentMembersAttribute{T}"/>
public class ComponentMembersAttribute : AutoUntypedMethodComponentRegistrationAttribute
{ }

/// <summary>
/// Represents an attribute used to register a callback method for a specific component type.
/// The callback method annotated with this attribute must be static and conform to predefined
/// parameters in order to interact with the specified component during runtime processing.
///
/// This will be called when the component is registered with <see cref="PolyWorld.Register{T}"/>
/// </summary>
/// <typeparam name="T">The type of component associated with the callback registration.</typeparam>
public class RegistrationCallbackAttribute<T> : AutoMethodComponentRegistrationAttribute<T>
{ }

/// <inheritdoc cref="RegistrationCallbackAttribute{T}"/>
public class RegistrationCallbackAttribute : AutoUntypedMethodComponentRegistrationAttribute
{ }