using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PolyECS.Generator;

public static class SyntaxHelpers
{
    public static bool IsPartial(this TypeDeclarationSyntax syntax, CancellationToken cancel = default)
    {
        foreach (var modifier in syntax.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PartialKeyword))
            {
                return true;
            }
            cancel.ThrowIfCancellationRequested();
        }
        return false;
    }

    public static List<MethodDeclarationSyntax> MethodWithAttribute(
        this TypeDeclarationSyntax declaration,
        string attributeName,
        CancellationToken cancel = default
    )
    {
        var methods = new List<MethodDeclarationSyntax>();
        foreach (var member in declaration.Members)
        {
            cancel.ThrowIfCancellationRequested();
            if (member is not MethodDeclarationSyntax method)
            {
                continue;
            }
            if (method.AttributeLists.SelectMany(x => x.Attributes).Any(x => x.Name.ToString() == attributeName))
            {
                methods.Add(method);
            }
        }
        return methods;
    }

    public static Accessibility GetAccessModifier(this TypeDeclarationSyntax syntax, CancellationToken cancel = default)
    {
        foreach (var modifier in syntax.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PublicKeyword))
            {
                return Accessibility.Public;
            }
            if (modifier.IsKind(SyntaxKind.InternalKeyword))
            {
                return Accessibility.Internal;
            }
            if (modifier.IsKind(SyntaxKind.ProtectedKeyword))
            {
                return Accessibility.Protected;
            }
            if (modifier.IsKind(SyntaxKind.PrivateKeyword))
            {
                return Accessibility.Private;
            }
            cancel.ThrowIfCancellationRequested();
        }
        return Accessibility.Internal;
    }
    
    public static AttributeSyntax GetNamedAttribute(this MethodDeclarationSyntax node, string attributeName, CancellationToken cancel = default)
    {
        foreach (var attribute in node.AttributeLists.SelectMany(x => x.Attributes))
        {
            if (attribute.Name.ToString() == attributeName)
            {
                return attribute;
            }
            cancel.ThrowIfCancellationRequested();
        }
        return null;
    }
    
    public static List<UsingDirectiveSyntax> GetFileUsings(this SyntaxNode node, CancellationToken cancel = default)
    {
        var usings = new List<UsingDirectiveSyntax>();
        var current = node;
        while (current is not null)
        {
            if (current is CompilationUnitSyntax unit)
            {
                foreach (var import in unit.Usings)
                {
                    usings.Add(import);
                }
                return usings;
            }
            current = current.Parent;
            cancel.ThrowIfCancellationRequested();
        }
        return usings;
    }
}
