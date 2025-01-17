namespace PolyGame.Assets;

public struct Handle<T>
{
    public static Handle<T> Invalid = new(-1, 0);
    private readonly int _index;
    private readonly ushort _generation;

    internal Handle(int index, ushort generation)
    {
        _index = index;
        _generation = generation;
    }

    public int Index() => _index;

    public ushort Generation() => _generation;

    public bool Valid() => _generation != 0 && _index >= 0;

    public Handle<T> Clone(AssetServer server)
    {
        if (!Valid())
            return Invalid;
        return server.Clone(this);
    }
}
