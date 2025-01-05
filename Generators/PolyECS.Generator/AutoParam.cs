using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeGenHelpers;
using CodeGenHelpers.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PolyECS.Generator;

public class AutoParam
{
    public AutoParamKind Kind;
    public MethodDeclarationSyntax RunMethod;
    public string ParamName;
    public WriteAnnotation Annotation = WriteAnnotation.ReadWrite; // Assume readwrite by default
    public ParameterSyntax Param;
    public bool IsOptional;
    public Modifier ParamModifier;
    public ITypeSymbol TypeInfo;
    public string TypeName;
    public bool HasAttribute;
    public int Index;

    public enum Modifier
    {
        None,
        Ref,
        RefReadonly,
        In,
        Out
    }


    public static string ModifierToString(Modifier modifier)
    {
        return modifier switch
        {
            Modifier.None => "",
            Modifier.Ref => "ref",
            Modifier.RefReadonly => "ref readonly",
            Modifier.In => "in",
            Modifier.Out => "out",
            _ => ""
        };
    }


    /// <summary>
    /// If the parent type is a nullable/wrapper type, this will be the underlying type.
    /// </summary>
    public ITypeSymbol BaseTypeInfo;


    public string PropertyName => $"_p_{ParamName}";

    /// <summary>
    /// Returns true if the type implements IStaticSystemParam
    /// </summary>
    public static bool IsStaticSystemParam(ITypeSymbol type)
    {
        return type.AllInterfaces.Any(x =>
            x.Name == "IStaticSystemParam" &&
            x.TypeArguments.Length == 1 &&
            SymbolEqualityComparer.Default.Equals(x.TypeArguments[0], type)
        );
    }

    public static (AutoParam?, Diagnostic?) Parse(GeneratorSyntaxContext ctx, TypeDeclarationSyntax declaration,
        MethodDeclarationSyntax runMethod,
        ParameterSyntax param, int index)
    {
        if (param.Type is null)
            return (null, Diagnostic.Create(Diagnostics.AutoParamInternalError, param.GetLocation()));

        var model = ctx.SemanticModel.GetTypeInfo(param.Type);
        if (model.Type is null)
            return (null, Diagnostic.Create(Diagnostics.AutoParamInternalError, param.GetLocation()));

        var typeInfo = model.Type;
        var providers = GetParamProviders(declaration, param.Identifier.Text);
        if (providers.Count > 1)
        {
            return (null, Diagnostic.Create(Diagnostics.AutoParamMultipleProviders, param.GetLocation()));
        }

        var modifier = Modifier.None;
        // I don't think this is possible?
        if (param.Modifiers.Count > 1)
        {
            return (null, Diagnostic.Create(Diagnostics.AutoParamInternalError, param.GetLocation()));
        }

        if (param.Modifiers.Count == 1)
        {
            var mod = param.Modifiers[0].Text;
            if (mod == "ref")
            {
                modifier = Modifier.Ref;
            }
            else if (mod == "in")
            {
                modifier = Modifier.In;
            }
            else if (mod == "out")
            {
                modifier = Modifier.Out;
            }
            else if (mod == "ref readonly")
            {
                modifier = Modifier.RefReadonly;
            }
            else
            {
                return (null, Diagnostic.Create(Diagnostics.AutoParamInternalError, param.GetLocation()));
            }
        }

        if (modifier == Modifier.Out)
        {
            return (null, Diagnostic.Create(Diagnostics.AutoParamOutParam, param.GetLocation()));
        }

        return (new AutoParam(param, typeInfo, runMethod, providers.Count > 0 ? providers[0] : null, modifier, index),
            null);
    }

    public static List<MethodDeclarationSyntax> GetParamProviders(TypeDeclarationSyntax declaration, string paramName)
    {
        var candidateGetters = declaration.MethodWithAttribute("ParamProvider");
        var matchingGetters = new List<MethodDeclarationSyntax>();
        foreach (var candidate in candidateGetters)
        {
            if (candidate.ParameterList.Parameters.Count() > 1) continue;
            if (candidate.ParameterList.Parameters.Count() == 1 &&
                candidate.ParameterList.Parameters[0].Type.ToString() != "ParamBuilder") continue;
            foreach (var annotation in candidate.AttributeLists.SelectMany(x => x.Attributes))
            {
                if (annotation.Name.ToString().Equals("ParamProvider"))
                {
                    var arg = annotation.ArgumentList?.Arguments.FirstOrDefault();
                    if (arg is null)
                        continue;
                    var name = Regex.Unescape(arg.Expression.ToString());
                    name = name.Trim('"');
                    if (name.Equals(paramName))
                    {
                        matchingGetters.Add(candidate);
                    }
                }
            }
        }

        return matchingGetters;
    }

    public AutoParam(ParameterSyntax param, ITypeSymbol typeInfo, MethodDeclarationSyntax runMethod,
        MethodDeclarationSyntax? provider, Modifier modifier, int index)
    {
        Index = index;
        Param = param;
        RunMethod = runMethod;
        ParamName = param.Identifier.Text;
        TypeInfo = typeInfo;
        IsOptional = TypeInfo.IsNullable();
        BaseTypeInfo = TypeInfo;
        ParamModifier = modifier;
        Annotation = WriteAnnotation.ReadWrite;
        HasAttribute = param.AttributeLists.Any();


        if (IsOptional && TypeInfo is INamedTypeSymbol { TypeArguments.Length: 1 } namedType)
        {
            BaseTypeInfo = namedType.TypeArguments[0];
        }

        TypeName = TypeInfo.GetTypeName();
        if (provider != null)
        {
            Kind = AutoParamKind.MethodProvided;
            Provider = provider;
        }
        else if (IsStaticSystemParam(typeInfo))
        {
            Kind = AutoParamKind.IInto;
        }
        else if (typeInfo.Name == "Query")
        {
            Kind = AutoParamKind.Query;
        }
        else
        {
            Kind = AutoParamKind.Res;
            var hasInAttribute = param.AttributeLists.Any(x => x.Attributes.Any(y => y.Name.ToString() == "In" || y.Name.ToString() == "InAttribute"));
            if (modifier == Modifier.In || modifier == Modifier.RefReadonly || hasInAttribute)
            {
                Annotation = WriteAnnotation.ReadOnly;
                TypeName = "Res<" + TypeName + ">";
            }
            else
            {
                TypeName = "ResMut<" + TypeName + ">";
            }
        }
    }

    public MethodDeclarationSyntax? Provider;

    public void ParamCreatorCode(ICodeWriter b)
    {
        switch (Kind)
        {
            case AutoParamKind.MethodProvided when Provider!.ParameterList.Parameters.Count == 0:
                b.AppendLine($"{PropertyName} = {Provider!.Identifier.ToString()}();");
                b.AppendLine($"builder.With({PropertyName});");
                break;
            case AutoParamKind.MethodProvided:
                b.AppendLine($"{PropertyName} = {Provider!.Identifier.ToString()}(builder);");
                break;
            case AutoParamKind.Res when WriteAnnotation.ReadWrite == Annotation:
                b.AppendLine($"{PropertyName} = builder.ResMut<{TypeInfo.GetTypeName()}>();");
                break;
            case AutoParamKind.Res:
                b.AppendLine($"{PropertyName} = builder.Res<{TypeInfo.GetTypeName()}>();");
                break;
            case AutoParamKind.IInto:
                b.AppendLine($"{PropertyName} = builder.With<{TypeName}>();");
                break;
            case AutoParamKind.Query:
                var typeString = "";
                var namedType = TypeInfo as INamedTypeSymbol;
                if (namedType?.TypeArguments.Any() ?? false)
                {
                    var genericArgs = string.Join(",", namedType.TypeArguments.Select(x => x.GetTypeName()));
                    typeString = $"<{genericArgs}>";
                }

                if (!HasAttribute)
                    b.AppendLine($"{PropertyName} = builder.Query{typeString}();");
                else
                {
                    b.AppendLine($"{PropertyName} = builder.QueryBuilder{typeString}(b =>");
                    using (b.Block(""))
                    {
                        using (b.Block("unsafe"))
                        {
                            b.AppendLine("QueryBuilder qb = new(b.World);");
                            b.AppendLine("qb.Desc = b.Desc;");
                            // TODO - should probably change this to be more precise in case theres multiple methods.
                            b.AppendLine(
                                $"var queryAttrs = GetType().GetMethod(\"{RunMethod!.Identifier}\", [{string.Join(", ", RunMethod.ParameterList.Parameters.Select(x => $"typeof({x.Type?.ToString()})"))}])?.GetParameters()[{Index}].GetCustomAttributes(true).OfType<QueryBuilderAttribute>();");
                            b.ForEach("var attr", "queryAttrs").WithBody(fb => fb.AppendLine("qb = attr.Apply(qb);"));
                            b.AppendLine("b.Desc = qb.Desc;");
                        }

                        b.AppendLine("return b;");
                    }

                    b.AppendLine(");");
                }

                break;
            default:
                b.AppendLine($"{PropertyName} = null");
                break;
        }
    }

    public string ParamGetterCode()
    {
        if (ParamModifier != Modifier.None)
            return ModifierToString(ParamModifier) + " l_" + ParamName;
        if (Kind == AutoParamKind.Res && (TypeInfo.Name != "Res" && TypeInfo.Name != "ResMut"))
            return $"{PropertyName}.Get()";
        return $"{PropertyName}";
    }

    public void RunPreconditionCode(ICodeWriter writer)
    {
        if (ParamModifier == Modifier.None) return;
        if (Kind == AutoParamKind.Res && (TypeInfo.Name != "Res" && TypeInfo.Name != "ResMut"))
            writer.AppendLine("var l_" + ParamName + " = " + PropertyName + ".Get();");
        else
            writer.AppendLine("var l_" + ParamName + " = " + PropertyName + ";");
    }
}

public enum WriteAnnotation
{
    ReadOnly,
    ReadWrite,
}

public enum AutoParamKind
{
    MethodProvided,
    Res,
    IInto,
    Query,
}