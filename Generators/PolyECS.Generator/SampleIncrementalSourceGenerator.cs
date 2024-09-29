using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;


namespace PolyECS.Generator;


[Generator]
public class AutoSystemGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add marker attributes to the compilation
        context.RegisterPostInitializationOutput(ctx
            => ctx.AddSource("AutoSystemAttributes.g.cs", SourceText.From(AttributeGenerationHelper.Attributes, Encoding.UTF8)));

        var autoSystems = context.SyntaxProvider.CreateSyntaxProvider(
            (static (s, _) => s is ClassDeclarationSyntax { BaseList.Types.Count: > 0 }), // Filter to classes with base types
            AutoSystemBuilder.FromCtx // Filter and parse out all classes that are actually autosystems
        );
        
        // Report any diagnostic failures
        context.RegisterSourceOutput(autoSystems.Where(x => x.Diagnostics.Count > 0).Select((x, _) => x.Diagnostics), Report);

        // Build any valid parses
        context.RegisterSourceOutput(autoSystems.Where(x => x.Value is not null).Select((x, _) => x.Value!), Build);
    }
    
    private static void Report(SourceProductionContext ctx, List<Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
        {
            ctx.ReportDiagnostic(diagnostic);
        }
    }
    
    private static void Build(SourceProductionContext ctx, AutoSystemBuilder systemBuilder)
    {
        var file = systemBuilder.Generate(ctx);
        var path = $"{systemBuilder.Namespace}.{systemBuilder.SystemName}.g.cs";
        ctx.AddSource(path, SourceText.From(file.Build(), Encoding.UTF8));
    }
}
