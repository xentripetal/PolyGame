using System.Reflection.Metadata;

namespace PolyGame;

public struct Handle<T> : IDisposable
{
    readonly AssetPath _path;
    int _id;
    private AssetServer _server;

    internal Handle(AssetServer server, AssetPath path, int id)
    {
        _server = server;
        _path = path;
        _id = id;
    }

    public int Id()
    {
        return _id;
    }

    public AssetPath Path()
    {
        return _path;
    }

    public bool Valid()
    {
        return _id > 0;
    }

    public T? Get()
    {
        if (_server == null)
        {
            return default;
        }
        return _server.Get(this);
    }

    public bool IsLoaded()
    {
        if (_server == null)
        {
            return false;
        }
        return _server.IsLoaded(this);
    }

    public void Dispose()
    {
        if (_server != null)
        {
            _server.Release(this);
            _server = null;
            _id = -1;
        }
    }
}
