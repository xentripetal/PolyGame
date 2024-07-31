using System.Reflection.Metadata;

namespace PolyGame;

public struct Handle<T>
{
    public static Handle<T> Invalid = new Handle<T>(-1, 0);
    private int _index;
    private ushort _generation;

    internal Handle(int index, ushort generation)
    {
        _index = index;
        _generation = generation;
    }

    public int Index()
    {
        return _index;
    }

    public ushort Generation()
    {
        return _generation;
    }

    public bool Valid()
    {
        return _generation != 0 && _index >= 0;
    }

    public Handle<T> Clone(AssetServer server)
    {
        if (!Valid())
            return Invalid;
        return server.Clone(this);
    }
}
