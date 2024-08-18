using PolyGame.Assets;

namespace PolyGame.Tests.Assets;

public class AssetPathTests
{
    [Fact]
    public void TestAssetPathNoScheme()
    {
        var path = new AssetPath("example.test");
        Assert.Equal("example.test", path.Path);
        Assert.Equal("file", path.Scheme);
        Assert.Equal("test", path.Extension);
        Assert.Null(path.Label);
        Assert.Equal("file://example.test", path.ToString());
    }

    [Fact]
    public void TestAssetPathLabel()
    {
        var path = new AssetPath("example.a.b#label");
        Assert.Equal("example.a.b", path.Path);
        Assert.Equal("file", path.Scheme);
        Assert.Equal("b", path.Extension);
        Assert.Equal("label", path.Label);
        Assert.Equal("file://example.a.b#label", path.ToString());
    }

    [Fact]
    public void TestAssetPathScheme()
    {
        var path = new AssetPath("http://example.com/test");
        Assert.Equal("example.com/test", path.Path);
        Assert.Equal("http", path.Scheme);
        Assert.Equal("", path.Extension);
        Assert.Null(path.Label);
        Assert.Equal("http://example.com/test", path.ToString());
    }
}
