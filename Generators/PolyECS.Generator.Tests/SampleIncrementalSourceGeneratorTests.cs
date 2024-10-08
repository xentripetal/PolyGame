using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace PolyECS.Generator.Tests;

public class AutoSystemGeneratorTests
{
    private const string VectorClassText = @"
using PolyECS;
using PolyECS.Systems;

namespace TestNamespace;

public partial class TestSystem : AutoSystem
{
    [AutoRunMethod]
    public void Run(PolyWorld world) {
    }
}";

    private const string GeneratedHeader = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------";

    private const string ExpectedGeneratedClassText = $@"{GeneratedHeader}

using PolyECS;
using PolyECS.Systems;

namespace TestNamespace
{{
    public partial class TestSystem : PolyECS.AutoSystem
    {{
        private PolyWorldParam _p_world;

        public override PolyECS.Systems.ISystemParam[] GetParams(PolyECS.PolyWorld world)
        {{
            _p_world = (PolyWorldParam)PolyWorld.IntoParam(world);
            return [_p_world];
        }}

        public override PolyECS.Empty Run(PolyECS.Empty e, PolyWorld world)
        {{
            Run(_p_world.Get(world, Meta));
            return e;
        }}
    }}
}}
";

    [Fact]
    public void GenerateReportMethod()
    {
        // Create an instance of the source generator.
        var generator = new AutoSystemGenerator();

        // Source generators should be tested using 'GeneratorDriver'.
        var driver = CSharpGeneratorDriver.Create(generator);

        // We need to create a compilation with the required source code.
        var compilation = CSharpCompilation.Create(nameof(AutoSystemGeneratorTests),
            new[]
            {
                CSharpSyntaxTree.ParseText(VectorClassText)
            },
            new[]
            {
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        // Run generators and retrieve all results.
        var runResult = driver.RunGenerators(compilation).GetRunResult();

        // All generated files can be found in 'RunResults.GeneratedTrees'.
        var generatedFileSyntax = runResult.GeneratedTrees.Single(t => t.FilePath.EndsWith("TestSystem.g.cs"));

        // Complex generators should be tested using text comparison.
        Assert.Equal(ExpectedGeneratedClassText, generatedFileSyntax.GetText().ToString(),
            ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
    }
}
