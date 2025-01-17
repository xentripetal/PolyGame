namespace PolyECS.Generator.Tests;

public static class Examples
{
    public static readonly string NotPartial = @"
namespace CSharpCodeGen;
using PolyECS;

internal class NonPartialSystem : AutoSystem {
    public void Run() { }
}
";

    public static readonly string NoRunMethod = @"
namespace CSharpCodeGen;
using PolyECS;

internal partial class PartialSystem : AutoSystem {
}
";

    public static readonly string ValidNoParams = @"
namespace CSharpCodeGen;
using PolyECS;

internal partial class NonPartialSystem : AutoSystem {
    public void Run() { }
}
";

    public static readonly string QuickTest = @"
using Flecs.NET.Core;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;

namespace PolyGame.Graphics.Renderers;

public partial class TestAutoRenderer : AutoSystem
{
    public void Run(
        PolyWorld world,
        Query<int> cameras,
    ) { }
}";
}
