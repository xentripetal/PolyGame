namespace PolyGame.Assets;

/// <summary>
///     System for loading assets from a <see cref="AssetPath" />. Each loader is responsible for caching its own assets.
/// </summary>
public interface IAssetLoader
{
    public IEnumerable<string> SupportedExtensions { get; }
    public T Load<T>(AssetPath path);
    public void Unload(AssetPath path, object asset);
}
