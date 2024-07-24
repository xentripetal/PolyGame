using Microsoft.Xna.Framework.Content;

namespace PolyGame;

public class XNBAssetLoader : IAssetLoader
{
    public XNBAssetLoader(ContentManager manager)
    {
        Manager = manager;
    }

    protected ContentManager Manager;
    protected Dictionary<string, int> _referenceCount = new Dictionary<string, int>();
    public IEnumerable<string> SupportedExtensions
    {
        get => ["", "xnb"];
    }

    public T Load<T>(AssetPath path)
    {
        // multiple unique assets can be loaded from the same path
        var data = Manager.Load<T>(path.Path);
        if (!_referenceCount.TryAdd(path.Path, 1))
        {
            _referenceCount[path.Path]++;
        }
        return data;
    }

    public void Unload(AssetPath path, object asset)
    {
        if (_referenceCount.TryGetValue(path.Path, out var count))
        {
            if (count == 1)
            {
                Manager.UnloadAsset(path.Path);
                _referenceCount.Remove(path.Path);
            }
            else
            {
                _referenceCount[path.Path]--;
            }
        }
    }
}
