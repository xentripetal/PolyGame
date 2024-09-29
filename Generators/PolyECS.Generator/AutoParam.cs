using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeGenHelpers.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PolyECS.Generator;

public class AutoParam
{
    public AutoParamKind Kind;
    public string ParamName;
    public WriteAnnotation Annotation = WriteAnnotation.ReadWrite; // Assume readwrite by default
    public ParameterSyntax Param;
    public bool IsOptional;
    public bool IsRef;
    public ITypeSymbol TypeInfo;
    /// <summary>
    /// If the parent type is a nullable/wrapper type, this will be the underlying type.
    /// </summary>
    public ITypeSymbol BaseTypeInfo;

    public static Dictionary<string, string> KnownIIntoTypes = new ()
    {
        {"World", "WorldParam"},
        {"PolyWorld","PolyWorldParam"},
        {"TQuery", "TQueryParam"}
    };

    public string PropertyName => $"_p_{ParamName}";

    public AutoParam(GeneratorSyntaxContext ctx, TypeDeclarationSyntax declaration, ParameterSyntax param)
    {
        Param = param;
        ParamName = param.Identifier.Text;
        var model = ctx.SemanticModel.GetTypeInfo(param.Type);
        TypeInfo = model.Type;
        IsOptional = TypeInfo.IsNullable();
        BaseTypeInfo = TypeInfo;
        IsRef = param.Modifiers.Any(x => x.Text== "ref");
        if (IsOptional && TypeInfo is INamedTypeSymbol { TypeArguments.Length: 1 } namedType)
        {
            BaseTypeInfo = namedType.TypeArguments[0];
        }
        if (KnownIIntoTypes.ContainsKey(BaseTypeInfo.Name))
        {
            Kind = AutoParamKind.IInto;
        }
        else
        {
            Kind = AutoParamKind.Res;
        }

        var candidateGetters = declaration.MethodWithAttribute("ParamProvider");
        foreach (var candidate in candidateGetters)
        {
            foreach (var annotation in candidate.AttributeLists.SelectMany(x => x.Attributes))
            {
                if (annotation.Name.ToString().Equals("ParamProvider"))
                {
                    var name = annotation.ArgumentList?.Arguments.FirstOrDefault()?.ToString();
                    // Remove quotes. 
                    name = Regex.Unescape(name);
                    name = name.Trim('"');
                    if (name.Equals(ParamName))
                    {
                        if (Getter != null)
                        {

                            throw new System.Exception("Multiple ParamProvider methods found for the same parameter");
                        }
                        Getter = candidate;
                        Kind = AutoParamKind.MethodProvided;
                    }
                }
            }
        }
    }

    public MethodDeclarationSyntax? Getter;

    public string ParamCreatorCode()
    {
        if (Kind == AutoParamKind.MethodProvided)
        {
            return $"{Getter.Identifier.ToString()}(world)";
        }
        // TODO optionals
        if (Kind == AutoParamKind.Res)
        {
            if (WriteAnnotation.ReadWrite == Annotation || WriteAnnotation.WriteOnly == Annotation)
            {
                return $"Param.OfResMut<{BaseTypeInfo.Name}>()";
            }
            return $"Param.OfRes<{BaseTypeInfo.Name}>()";
        }
        if (Kind == AutoParamKind.IInto)
        {
            if (BaseTypeInfo.Name.Equals("TQuery") && BaseTypeInfo is INamedTypeSymbol t)
                return "new()";

            return $"({ParamType()}){BaseTypeInfo.Name}.IntoParam(world)";
        }
        return "null";
    }
    
    public string ParamType()
    {
        switch (Kind)
        {
            case AutoParamKind.MethodProvided:
                return Getter.ReturnType.ToString();

            case AutoParamKind.Res:
                if (WriteAnnotation.ReadWrite == Annotation || WriteAnnotation.WriteOnly == Annotation)
                {
                    return $"ResMutParam<{BaseTypeInfo.Name}>";
                }
                return $"ResParam<{BaseTypeInfo.Name}>";

            case AutoParamKind.IInto:
                if (BaseTypeInfo.Name.Equals("TQuery") && BaseTypeInfo is INamedTypeSymbol t)
                {
                    return $"TQueryParam<{string.Join(", ", t.TypeArguments.Select(x => x.ToDisplayString()))}>";
                }
                return KnownIIntoTypes[BaseTypeInfo.Name];

            default:
                throw new System.Exception("Unknown AutoParamKind");
        }
    }

    public string ParamGetterCode()
    {
        if (IsRef && Kind == AutoParamKind.Res)
        {
            return $"ref _p_{ParamName}.Get(world, Meta).GetRef().Get()";
        }
        return $"_p_{ParamName}.Get(world, Meta)";
    }
}

public enum WriteAnnotation
{
    ReadOnly,
    WriteOnly,
    ReadWrite,
}

public enum AutoParamKind
{
    MethodProvided,
    Res,
    IInto,
}
