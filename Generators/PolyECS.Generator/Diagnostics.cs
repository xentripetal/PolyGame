using Microsoft.CodeAnalysis;

namespace PolyECS.Generator;

public class Diagnostics
{
    public static readonly DiagnosticDescriptor MissingPartial = new DiagnosticDescriptor(
        id: "PECS.G001",
        title: "Class or struct must be made partial",
        messageFormat: "Class or struct must be made partial",
        category: "Configuration",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingRunMethod = new DiagnosticDescriptor(
        id: "PECS.G002",
        title: "Class or struct must contain a Run Method",
        messageFormat: "Class or struct must contain a Run method annotated with AutoRunMethodAttribute",
        category: "Configuration",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MultipleRunMethods = new DiagnosticDescriptor(
        id: "PECS.G003",
        title: "Class or struct must contain only one Run Method",
        messageFormat: "Class or struct must contain only one Run method annotated with AutoRunMethodAttribute",
        category: "Configuration",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
