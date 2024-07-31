using Microsoft.Xna.Framework.Graphics;

namespace PolyGame;

public class ImageLoader : IAssetLoader
{
    public ImageLoader(GraphicsDevice graphicsDevice)
    {
        this.GraphicsDevice = graphicsDevice;
    }
    
    protected GraphicsDevice GraphicsDevice;
    
    public IEnumerable<string> SupportedExtensions { get; } = new[]
    {
        "png"
    };
    
    protected Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

    public T Load<T>(AssetPath path)
    {
        if (path.Scheme != "file")
        {
            throw new NotSupportedException($"Attempted to load image from unsupported scheme {path.Scheme}");;
        }
        if (typeof(T) != typeof(Texture2D))
        {
            throw new NotSupportedException($"Attempted to load image as unsupported type {typeof(T)}");
        }
        if (_textures.TryGetValue(path.Path, out var texture))
        {
            return (T)(object)texture;
        }
        var tex = Texture2D.FromFile(GraphicsDevice, path.Path);
        _textures[path.Path] = tex;
        return (T)(object)tex;
    }

    public void Unload(AssetPath path, object asset)
    {
        if (path.Scheme != "file")
        {
            throw new NotSupportedException($"Attempted to unload image from unsupported scheme {path.Scheme}");
        }
        
        if (asset is not Texture2D tex)
        {
            throw new NotSupportedException($"Attempted to unload image as unsupported type {asset.GetType()}");
        }
        
        if (_textures.TryGetValue(path.Path, out var texture) && texture == tex)
        {
            _textures.Remove(path.Path);
            tex.Dispose();
        }
    }
}
