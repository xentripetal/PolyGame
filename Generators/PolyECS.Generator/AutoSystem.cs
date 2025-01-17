using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using CodeGenHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PolyECS.Generator;

/// <summary>
/// Helper wrapper for a AutoSystem with support for reporting diagnostics for parse failures
/// </summary>
internal struct ParsedAutoSystem
{
    public static ParsedAutoSystem Empty()
    {
        return new ParsedAutoSystem();
    }

    public static ParsedAutoSystem Valid(AutoSystemBuilder builder)
    {
        return new ParsedAutoSystem
        {
            Value = builder
        };
    }

    public static ParsedAutoSystem Err(Diagnostic err)
    {
        var parsed = new ParsedAutoSystem();
        parsed.Diagnostics.Add(err);
        return parsed;
    }


    public AutoSystemBuilder? Value = null;
    public List<Diagnostic> Diagnostics = new();

    public ParsedAutoSystem() { }
}

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


        var runMethods = syntax.MethodWithAttribute("AutoRunMethod", cancellationToken);
        if (runMethods.Count == 0)
        {
            runMethods = syntax.MethodsNamed("Run", cancellationToken);
        }

        if (runMethods.Count > 1)
        {
            return ParsedAutoSystem.Err(Diagnostic.Create(Diagnostics.MultipleRunMethods, syntax.GetLocation()));
        }
        if (runMethods.Count == 0)
        {
            return ParsedAutoSystem.Err(Diagnostic.Create(Diagnostics.MissingRunMethod, syntax.GetLocation()));
        }
        var runMethod = runMethods[0];

        List<AutoParam> autoParams = new();
        for (var index = 0; index < runMethod.ParameterList.Parameters.Count; index++)
        {
            var p = runMethod.ParameterList.Parameters[index];
            var (param, err) = AutoParam.Parse(ctx, syntax, runMethod, p, index);
            if (err is not null)
            {
                return ParsedAutoSystem.Err(err);
            }

            autoParams.Add(param!);
        }

        return ParsedAutoSystem.Valid(new AutoSystemBuilder
        {
            Syntax = syntax,
            RunMethod = runMethod,
            Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
            Params = autoParams
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
            file.AddNamespaceImport(use.Name.ToString());
        }
        // Manually add Systems
        file.AddNamespaceImport("PolyECS.Systems");
        file.AddNamespaceImport("PolyECS");

        // If its a nested class, we need to add the parent class
        var cur = Syntax.Parent;
        ClassBuilder ext = null;
        while (cur is TypeDeclarationSyntax parent)
        {
            if (ext is null)
                ext = file.AddClass(parent.Identifier.ToString()).WithAccessModifier(parent.GetAccessModifier());
            else
            {
                ext = ext.AddNestedClass(parent.Identifier.ToString(), true, parent.GetAccessModifier());
            }
            cur = cur.Parent;
        }

        if (ext is null)
            ext = file.AddClass(SystemName).WithAccessModifier(Syntax.GetAccessModifier());
        else
            ext = ext.AddNestedClass(SystemName, true, Syntax.GetAccessModifier());
        ext.SetBaseClass("AutoSystem");

        // Start building the class
        foreach (var p in Params)
        {
            ext.AddProperty(p.PropertyName).WithAccessModifier(Accessibility.Private).SetType(p.TypeName);
        }
        ext.AddMethod("BuildParameters", Accessibility.Protected).Override().AddParameter("ParamBuilder", "builder")
            .WithBody(b =>
            {
                foreach (var p in Params) p.ParamCreatorCode(b);
            });

        ext.AddMethod("Run", Accessibility.Public).Override()
            .AddParameter("PolyWorld", "world").WithBody(b =>
            {
                foreach (var p in Params)
                {
                    p.RunPreconditionCode(b);
                }
                b.AppendLine($"{RunMethod.Identifier.ToString()}({string.Join(", ", Params.Select(p => p.ParamGetterCode()))});");
            });

        return file;
    }
}
