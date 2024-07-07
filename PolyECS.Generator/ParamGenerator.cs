using CodeGenHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PolyECS.Generator;

[Generator]
public sealed class ParamGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(postContext => {
            postContext.AddSource("PolyECS.Systems.SystemParams.g.cs", GenerateParams(4, 10));
        });
    }

    string GenerateParams(int startCount, int endCount)
    {
        var builder = CodeBuilder.Create("PolyECS.Systems")
            .AddNamespaceImport("Flecs.NET.Core");

        for (int i = startCount; i < endCount; i++)
        {
            BuildMultiParam(builder, i);
        }
        return builder.Build();
    }

    void BuildMultiParam(CodeBuilder file, int numParams)
    {
        var Class = file.AddClass($"MultiParam{numParams}").WithAccessModifier(Accessibility.Public);
        for (int i = 1; i <= numParams; i++)
        {
            Class.AddGeneric($"T{i}");
        }
        var typeString = string.Join(", ", Enumerable.Range(1, numParams).Select(i => $"T{i}"));
        Class.AddInterface($"ISystemParam<({typeString})>");
        var ctor = Class.AddConstructor();
        for (int i = 1; i <= numParams; i++)
        {
            ctor.AddParameter($"ISystemParam<T{i}>", $"p{i}");
        }
        ctor.WithBody(b => {
            var paramText = string.Join(", ", Enumerable.Range(1, numParams).Select(i => $"p{i}"));
            b.AppendLine($"_params = ({paramText});");
        });

        var paramType = string.Join(", ", Enumerable.Range(1, numParams).Select(i => $"ISystemParam<T{i}>"));
        Class.AddProperty("_params", Accessibility.Private).SetType($"({paramType})");

        Class.AddMethod("Initialize", Accessibility.Public)
            .AddParameter("PolyWorld", "world")
            .AddParameter("SystemMeta", "meta")
            .WithBody(b => {
                for (int i = 1; i <= numParams; i++)
                {
                    b.AppendLine($"_params.Item{i}.Initialize(world, meta);");
                }
            });

        Class.AddMethod("EvaluateNewTable", Accessibility.Public)
            .AddParameter("SystemMeta", "meta")
            .AddParameter("Table", "table")
            .AddParameter("int", "tableGen")
            .WithBody(b => {
                for (int i = 1; i <= numParams; i++)
                {
                    b.AppendLine($"_params.Item{i}.EvaluateNewTable(meta, table, tableGen);");
                }
            });

        Class.AddMethod("Get", Accessibility.Public)
            .WithReturnType($"({typeString})")
            .AddParameter("PolyWorld", "world")
            .AddParameter("SystemMeta", "meta")
            .WithBody(b => {
                var paramText = string.Join(", ", Enumerable.Range(1, numParams).Select(i => $"_params.Item{i}.Get(world, meta)"));
                b.AppendLine($"return ({paramText});");
            });
    }
}
