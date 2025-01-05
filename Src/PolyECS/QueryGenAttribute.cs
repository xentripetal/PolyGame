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

/// <summary>
/// Used with <see cref="Query"/> parameters for <see cref="PolyECS.Systems.AutoSystem"/> to modify the query without
/// using a ParamProvider.
/// </summary>
/// <param name="terms">Manual Term Modifiers per each term of the query</param>
/// <param name="fill">If a query has terms not defined in the terms parameter, the remaining terms will be filled with this modifier.</param>
[AttributeUsage(AttributeTargets.Parameter)]
public class QueryGenAttribute(TermMod[] terms = null, TermMod fill = TermMod.Default) : QueryBuilderAttribute
{
    public readonly TermMod[] Terms = terms ?? [];
    public readonly TermMod Fill = fill;

    public override QueryBuilder Apply(QueryBuilder builder)
    {
        var termCount = builder.GetTermCount();
        for (int i = 0; i < Math.Min(Terms.Length, termCount); i++)
        {
            builder = builder.TermAt(i);
            builder = ApplyTerm(builder, Terms[i]);
        }

        if (termCount > Terms.Length)
        {
            for (int i = Terms.Length; i < termCount; i++)
            {
                builder = builder.TermAt(i);
                builder = ApplyTerm(builder, Fill);
            }
        }

        return builder;
    }


    protected QueryBuilder ApplyTerm(QueryBuilder builder, TermMod mod)
    {
        if (mod.HasFlag(TermMod.InOut))
            builder = builder.InOut();
        else if (mod.HasFlag(TermMod.In))
            builder = builder.In();
        else if (mod.HasFlag(TermMod.Out))
            builder = builder.Out();

        if (mod.HasFlag(TermMod.Optional))
            return builder.Optional();
        return builder;
    }
}

[Flags]
public enum TermMod : int
{
    Default = 0,
    In = 1,
    Out = 2,
    InOut = 3,
    Optional = 4,
}