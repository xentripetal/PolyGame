namespace PolyGame.Assets;

public struct AssetPath : IEquatable<AssetPath>, IFormattable
{
    /// <summary>
    ///     URI Scheme of the asset. file, http, etc.
    /// </summary>
    public string Scheme;
    /// <summary>
    ///     Path to the asset.
    /// </summary>
    public string Path;
    /// <summary>
    ///     File extension of the asset.
    /// </summary>
    public string Extension;
    /// <summary>
    ///     An optional label for referencing elements internal to the path asset.
    /// </summary>
    public string? Label;

    /// <summary>
    ///     Creates a separate asset path from the given full path.
    ///     resource.txt -> AssetPath(Path: "resource.txt")
    ///     http://example.com/resource.txt -> AssetPath(Source: "http://example.com", Path: "resource.txt")
    ///     Image/Sprites.Atlas#Sprite1 -> AssetPath(Path: "Image/Sprites.Atlas", Label: "Sprite1")
    /// </summary>
    /// <param name="fullPath"></param>
    public AssetPath(string fullPath)
    {
        var labelIndex = fullPath.LastIndexOf('#');
        if (labelIndex > 0)
        {
            Label = fullPath.Substring(labelIndex + 1);
            Path = fullPath.Substring(0, labelIndex);
        }
        else
        {
            Label = null;
            Path = fullPath;
        }

        (Scheme, Path, Extension) = SchemePathAndExtension(Path);
    }

    public AssetPath(string path, string label)
    {

        (Scheme, Path, Extension) = SchemePathAndExtension(path);
        Label = label;
    }

    public static (string, string, string) SchemePathAndExtension(string path)
    {
        var sourceIndex = path.IndexOf("://", StringComparison.Ordinal);
        var source = "file";
        if (sourceIndex > 0)
        {
            source = path.Substring(0, sourceIndex);
            path = path.Substring(sourceIndex + 3);
        }
        var extIndex = path.LastIndexOf('.');
        var ext = "";
        if (extIndex > 0)
        {
            ext = path.Substring(extIndex + 1).ToLower();
            if (ext.Length > 4)
            {
                ext = "";
            }
            else
            {
                foreach (var c in ext)
                {
                    if (!char.IsLetterOrDigit(c))
                    {
                        ext = "";
                        break;
                    }
                }
            }
        }
        return (source, path, ext);
    }

    public bool Equals(AssetPath other) => Scheme == other.Scheme && Path == other.Path && Extension == other.Extension && Label == other.Label;

    public override bool Equals(object? obj) => obj is AssetPath other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Scheme, Path, Extension, Label);

    public override string ToString() => $"{Scheme}://{Path}{(Label != null ? $"#{Label}" : "")}";

    public string ToString(string? format, IFormatProvider? formatProvider) => ToString().ToString(formatProvider);
}
