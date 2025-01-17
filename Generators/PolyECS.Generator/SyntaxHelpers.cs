using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CodeGenHelpers.Internals;
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
    ) => MethodsMatching(declaration,
        x => x.AttributeLists.SelectMany(y => y.Attributes).Any(y => y.Name.ToString() == attributeName || y.Name.ToString() == attributeName + "Attribute"), cancel);

    public static List<MethodDeclarationSyntax> MethodsNamed(
        this TypeDeclarationSyntax declaration,
        string methodName,
        CancellationToken cancel = default
    ) => MethodsMatching(declaration, x => x.Identifier.Text == methodName, cancel);

    public static List<MethodDeclarationSyntax> MethodsMatching(this TypeDeclarationSyntax declaration,
        Func<MethodDeclarationSyntax, bool> matcher, CancellationToken cancel = default)
    {
        var methods = new List<MethodDeclarationSyntax>();
        foreach (var member in declaration.Members)
        {
            cancel.ThrowIfCancellationRequested();
            if (member is not MethodDeclarationSyntax method)
            {
                continue;
            }

            if (matcher(method))
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

    public static AttributeSyntax GetNamedAttribute(this MethodDeclarationSyntax node, string attributeName,
        CancellationToken cancel = default)
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

    private static readonly Dictionary<string, string> _typeMappings = new Dictionary<string, string>
    {
        { "Boolean", "bool" },
        { "Byte", "byte" },
        { "SByte", "sbyte" },
        { "Char", "char" },
        { "Decimal", "decimal" },
        { "Double", "double" },
        { "Single", "float" },
        { "Int32", "int" },
        { "UInt32", "uint" },
        { "Int64", "long" },
        { "UInt64", "ulong" },
        { "Int16", "short" },
        { "UInt16", "ushort" },
        { "Object", "object" },
        { "String", "string" }
    };

    public static string GetTypeName(this ITypeSymbol symbol)
    {
        if (symbol.ContainingNamespace.Name == "System" && _typeMappings.TryGetValue(symbol.Name, out var name))
            return name;

        var sb = new StringBuilder();
        sb.Append(symbol.Name);
        if (symbol is INamedTypeSymbol namedType)
        {
            if (namedType?.TypeArguments.Any() ?? false)
            {
                var genericArgs = string.Join(",", namedType.TypeArguments.Select(GetTypeName));
                sb.Append($"<{genericArgs}>");
            }
        }

        return sb.ToString();
    }
}