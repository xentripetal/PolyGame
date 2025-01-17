using Flecs.NET.Core;

namespace PolyECS;

public abstract class QueryBuilderAttribute : System.Attribute
{
    public abstract QueryBuilder Apply(QueryBuilder builder);
}

/// <summary>
/// Marks a <see cref="AutoSystem"/> <see cref="Res{T}"/> parameter as read only. If not specified the parameter is assumed to be <see cref="ResMut{T}"/>
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class InAttribute : Attribute
{ }