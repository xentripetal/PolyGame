using System.Runtime.CompilerServices;

namespace PolyECS.Tests;

public class ReferenceTest
{
    [Fact]
    public void TestWhatIsReference()
    {
        Assert.False(RuntimeHelpers.IsReferenceOrContainsReferences<int>());
    }
}
