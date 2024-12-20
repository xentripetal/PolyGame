using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CodeGenHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PolyECS.Generator;

class AutoSystemBuilder
{
    public static ParsedAutoSystem FromCtx(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
    {
        var syntax = ctx.Node as TypeDeclarationSyntax;
        if (syntax is null)
            return ParsedAutoSystem.Empty();

        if (ctx.SemanticModel.GetDeclaredSymbol(syntax) is not { } classSymbol)
            return ParsedAutoSystem.Empty();

        if (classSymbol.BaseType.Name.ToString() != "AutoSystem")
            return ParsedAutoSystem.Empty();

        if (!syntax.IsPartial())
            return ParsedAutoSystem.Err(Diagnostic.Create(Diagnostics.MissingPartial, syntax.GetLocation()));


        var autoRunMethods = syntax.MethodWithAttribute("AutoRunMethod", cancellationToken);
        if (autoRunMethods.Count > 1)
        {
            return ParsedAutoSystem.Err(Diagnostic.Create(Diagnostics.MultipleRunMethods, syntax.GetLocation()));
        }
        if (autoRunMethods.Count == 0)
        {
            return ParsedAutoSystem.Err(Diagnostic.Create(Diagnostics.MissingRunMethod, syntax.GetLocation()));
        }

        return ParsedAutoSystem.Valid(new AutoSystemBuilder
        {
            Syntax = syntax,
            RunMethod = autoRunMethods[0],
            Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
            Params = autoRunMethods[0].ParameterList.Parameters.Select(x => new AutoParam(ctx, syntax, x)).ToList()
        });
    }

    public TypeDeclarationSyntax Syntax;
    public MethodDeclarationSyntax RunMethod;
    public List<AutoParam> Params;
    public string Namespace;
    public string SystemName => Syntax.Identifier.ToString();

    public CodeBuilder Generate(SourceProductionContext ctx)
    {
        var file = CodeBuilder.Create(Namespace);

        // copy all the imports from the source file. We can't prune it as we can't tell what parameter type corresponds to what namespace
        var usings = Syntax.GetFileUsings(ctx.CancellationToken);
        foreach (var use in usings)
        {
            if (use.Name is not null)
                file.AddNamespaceImport(use.Name.ToString());
        }
        // Manually add Systems
        file.AddNamespaceImport("PolyECS.Systems");

        // Start building the class
        var ext = file.AddClass(SystemName).WithAccessModifier(Syntax.GetAccessModifier()).SetBaseClass("PolyECS.AutoSystem");
        foreach (var p in Params)
        {
            ext.AddProperty(p.PropertyName).WithAccessModifier(Accessibility.Private).SetType(p.ParamType());
        }
        ext.AddMethod("GetParams", Accessibility.Public).WithReturnType("PolyECS.Systems.ISystemParam[]").Override().AddParameter("PolyECS.PolyWorld", "world")
            .WithBody(b => {
                foreach (var p in Params)
                {
                    b.AppendLine($"{p.PropertyName} = {p.ParamCreatorCode()};");
                }
                b.AppendLine($"return [{string.Join(", ", Params.Select(p => p.PropertyName))}];");
            });
        ext.AddMethod("Run", Accessibility.Public).WithReturnType("PolyECS.Empty").Override().AddParameter("PolyECS.Empty", "e")
            .AddParameter("PolyWorld", "world").WithBody(b => {
                foreach (var p in Params)
                {
                    if (p.Kind == AutoParamKind.Res && !p.IsOptional)
                    {
                        b.AppendLine($"if (!{p.PropertyName}.IsGettable(world, Meta))");
                        b.AppendLine("\t return e;");
                    }
                }
                b.AppendLine($"{RunMethod.Identifier.ToString()}({
                    string.Join(", ", Params.Select(p => p.ParamGetterCode()))
                });");
                b.AppendLine("return e;");
            });

        return file;
    }
}
