using CodeGenHelpers;
using Microsoft.CodeAnalysis;

namespace PolyECS.Internal.Generator;

[Generator]
public sealed class TQueryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(postContext =>
        {
            postContext.AddSource("PolyECS.TQuery.g.cs", GenerateParams(2, 15));
        });
    }

    private string GenerateParams(int startCount, int endCount)
    {
        var builder = CodeBuilder.Create("PolyECS")
            .AddNamespaceImport("Flecs.NET.Core").AddNamespaceImport("PolyECS.Systems");

        for (var i = startCount; i < endCount; i++)
        {
            BuildTQuery(builder, i);
            BuildTermIndex(builder, i);
        }

        return builder.Build();
    }

    private void BuildTermIndex(CodeBuilder file, int paramIndex)
    {
        var index = paramIndex - 1;
        var Class = file.AddClass($"Term{index}").IsStruct().WithAccessModifier(Accessibility.Public)
            .AddInterface("ITermFilter");
        Class.AddMethod("GetChildren", Accessibility.Public).WithReturnType("IEnumerable<object>")
            .AddParameter("ref QueryBuilder", "qb").WithBody(b =>
            {
                b.AppendLine("return System.Linq.Enumerable.Empty<object>();");
            });
        Class.AddMethod("Apply", Accessibility.Public).WithReturnType("ref QueryBuilder")
            .AddParameter("ref QueryBuilder", "qb").WithBody(
                b =>
                {
                    b.AppendLine($"return ref qb.TermAt({index});");
                });
    }

    private void BuildTQuery(CodeBuilder file, int numParams)
    {
        var Class = file.AddClass("TQuery").WithAccessModifier(Accessibility.Public);
        for (var i = 0; i < numParams; i++)
            Class.AddGeneric($"T{i}");
        Class.AddGeneric("TFilter", b => b.AddConstraint("struct"));
        var typeString = string.Join(", ", Enumerable.Range(0, numParams).Select(i => $"T{i}"));

        Class.AddConstructor().AddParameter("PolyWorld", "world").WithBody(b => b.AppendLine("Query = Build(world);"));
        Class.AddProperty("Query", Accessibility.Public).SetType($"Query<{typeString}>");

        Class.AddMethod("Build", Accessibility.Public).MakeStaticMethod().WithReturnType($"Query<{typeString}>")
            .AddParameter("PolyWorld", "world")
            .WithBody(b =>
                {
                    b.Append("QueryBuilder qb = QueryHelpers.ApplyFilter<TFilter>(ref world.QueryBuilder()");
                    foreach (var i in Enumerable.Range(0, numParams))
                    {
                        b.AppendUnindented(".With<T" + i + ">()");
                    }

                    b.AppendUnindented(", new TermStack());");
                    b.NewLine();
                    using (b.Block("unsafe"))
                        b.AppendLine($"return new Query<{typeString}>(qb.Build());");
                }
            );

        Class.AddInterfaces([$"IStaticSystemParam<TQuery<{typeString}, TFilter>>", "IIntoSystemParam"]);
        Class.AddMethod("BuildParamValue", Accessibility.Public).WithReturnType($"TQuery<{typeString}, TFilter>")
            .MakeStaticMethod().AddParameter("PolyWorld", "world").WithBody(b => b.AppendLine("return new(world);"));
        Class.AddMethod("GetParam", Accessibility.Public).MakeStaticMethod().WithReturnType("ISystemParam")
            .AddParameter("PolyWorld", "world").AddParameter($"TQuery<{typeString}, TFilter>", "value")
            .WithBody(b => b.AppendLine("return value.IntoParam(world);"));
        Class.AddMethod("IntoParam", Accessibility.Public).WithReturnType("ISystemParam").AddParameter("PolyWorld", "world").WithBody(b =>
        {
            using (b.Block("unsafe"))
                b.AppendLine("return new QueryParam(new Query(Query.Handle));");
        });

        Class.AddMethod("Each", Accessibility.Public)
            .WithInheritDoc($"Query{{{typeString}}}.Each(Ecs.EachRefCallback{{{typeString}}}>)")
            .AddParameter($"Ecs.EachRefCallback<{typeString}>", "cb").WithBody(b => b.AppendLine("Query.Each(cb);"));

        Class.AddMethod("Each", Accessibility.Public)
            .WithInheritDoc($"Query{{{typeString}}}.Each(Ecs.EachEntityRefCallback{{{typeString}}}>)")
            .AddParameter($"Ecs.EachEntityRefCallback<{typeString}>", "cb")
            .WithBody(b => b.AppendLine("Query.Each(cb);"));

        Class.AddMethod("Each", Accessibility.Public)
            .WithInheritDoc($"Query{{{typeString}}}>.Each(Ecs.EachIterRefCallback{{{typeString}}})")
            .AddParameter($"Ecs.EachIterRefCallback<{typeString}>", "cb")
            .WithBody(b => b.AppendLine("Query.Each(cb);"));

        Class.AddMethod("Count", Accessibility.Public).WithInheritDoc($"Query{{{typeString}}}>.Count")
            .WithReturnType("int")
            .WithBody(b => b.AppendLine("return Query.Count();"));
    }
}