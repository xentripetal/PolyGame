using Flecs.NET.Core;

namespace PolyGame.Graphics.Renderable;

public struct RenderableReference : IComparable<RenderableReference>
{
    public float SortKey;
    public int DrawFuncIndex;
    public Entity Entity;

    public int CompareTo(RenderableReference other) => SortKey.CompareTo(other.SortKey);
}

/// <summary>
/// Container of all renderables to be rendered. 
/// </summary>
public class RenderableList
{
    /// <summary>
    /// Creates a new RenderableList with the given capacity.
    /// </summary>
    /// <param name="initialLayers">How many initial layers to buffer for</param>
    /// <param name="layerCapacity">Initial capacity for each render layer</param>
    public RenderableList(int initialLayers = 16, int layerCapacity = 1024)
    {
        _renderables = new FastList<FastList<RenderableReference>>(initialLayers);
        for (int i = 0; i < layerCapacity; i++)
        {
            _renderables.Add(new FastList<RenderableReference>(layerCapacity));
        }

    }

    protected FastList<FastList<RenderableReference>> _renderables;

    public int Count { get; protected set; }


    public void Add(RenderableReference renderable, uint layer = 0)
    {
        Count++;
        int neededCapacity = (int)(layer - _renderables.Length + 1);
        if (neededCapacity > 0)
        {
            _renderables.EnsureCapacity(neededCapacity);
        }
        _renderables.Buffer[layer].Add(renderable);
    }

    public int NumLayers => _renderables.Length;

    public IEnumerable<int> GetLayers()
    {
        for (int i = 0; i < _renderables.Length; i++)
        {
            if (_renderables.Buffer[i].Length > 0)
                yield return i;
        }
    }

    public IEnumerable<RenderableReference> GetRenderables(int layer)
    {
        if (layer < 0 || layer >= _renderables.Length)
        {
            return new RenderableReference[]
                { };
        }

        return _renderables.Buffer[layer].Buffer.Take(_renderables.Buffer[layer].Length);
    }


    public void Sort()
    {
        for (int i = 0; i < _renderables.Length; i++)
        {
            _renderables.Buffer[i].Sort();
        }
    }


    /// <summary>
    /// Clears out added renderables. This does not clear out the draw funcs.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < _renderables.Length; i++)
        {
            _renderables.Buffer[i].Clear();
        }
    }
}
