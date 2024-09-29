namespace PolyECS.Generator;

public class AttributeGenerationHelper
{
    public const string Attributes = @"
namespace PolyECS {
    [AttributeUsage(AttributeTargets.Method)]
    public class AutoRunMethodAttribute : System.Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class ParamProviderAttribute(string name) : System.Attribute { }
}";
}
