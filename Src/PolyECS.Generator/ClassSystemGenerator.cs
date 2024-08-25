using CodeGenHelpers;
using Microsoft.CodeAnalysis;

namespace PolyECS.Generator;

[Generator]
public sealed class ClassSystemGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(postContext => {
            postContext.AddSource("PolyECS.Systems.ClassSystem.g.cs", GenerateClasses(2, 10));
        });
    }

    public string GenerateClasses(int startCount, int endCount)
    {
        var builder = CodeBuilder.Create("PolyECS.Systems")
            .AddNamespaceImport("Flecs.NET.Core");

        for (var i = startCount; i < endCount; i++)
        {
            BuildMultiParam(builder, i);
        }
        return builder.Build();
    }

    private void BuildMultiParam(CodeBuilder file, int numParams)
    {
        var Class = file.AddClass("ClassSystem").WithAccessModifier(Accessibility.Public).Abstract();
        for (var i = 1; i <= numParams; i++)
        {
            Class.AddGeneric($"T{i}");
        }
        var typeString = string.Join(", ", Enumerable.Range(1, numParams).Select(i => $"T{i}"));
        Class.SetBaseClass($"ClassSystem<({typeString})>");
        Class.AddConstructor()
            .WithBaseCall(new Dictionary<string, string>([KeyValuePair.Create("string", "name")]));

        Class.AddConstructor().WithBaseCall();

        Class.AddMethod("Run", Accessibility.Public).Override()
            .AddParameter($"({typeString})", "param")
            .WithBody(b => {
                b.AppendLine($"Run({string.Join(", ", Enumerable.Range(1, numParams).Select(i => $"param.Item{i}"))});");
            });

        var runMethod = Class.AddMethod("Run", Accessibility.Public).Abstract();
        for (var i = 1; i <= numParams; i++)
        {
            runMethod.AddParameter($"T{i}", $"param{i}");
        }

        var paramClass = "BiParam";
        if (numParams == 3)
        {
            paramClass = "TriParam";
        }
        else if (numParams > 3)
        {
            paramClass = $"MultiParam{numParams}";
        }

        var paramTypeString = string.Join(", ", Enumerable.Range(1, numParams).Select(i => $"ISystemParam<T{i}>"));
        Class.AddMethod("CreateParams", Accessibility.Protected).WithReturnType($"({paramTypeString})").AddParameter("PolyWorld", "world").Abstract();
        Class.AddMethod("CreateParam", Accessibility.Protected).Override().WithReturnType($"ISystemParam<({typeString})>").AddParameter("PolyWorld", "world")
            .WithBody(b => {
                b.AppendLine($"({paramTypeString}) p = CreateParams(world);");
                b.AppendLine($"return new {paramClass}<{typeString}>({string.Join(", ", Enumerable.Range(1, numParams).Select(i => $"p.Item{i}"))});");
            });

    }
}
