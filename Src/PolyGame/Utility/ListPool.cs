using System.Runtime.CompilerServices;

namespace PolyGame;

public class ListPool<T>
{
    /// <summary>
    /// direct access to the backing buffer. Do not use buffer.Length! Use FastList.length
    /// </summary>
    public T[] Buffer;
    protected Stack<int> _freeIndices = new Stack<int>();

    /// <summary>
    /// direct access to the length of the filled items in the buffer. Do not change.
    /// </summary>
    protected int Length = 0;

    public ListPool(int capacity)
    {
        Buffer = new T[capacity];
    }

    public ListPool() : this(5) { }

    /// <summary>
    /// provided for ease of access though it is recommended to just access the buffer directly.
    /// </summary>
    /// <param name="index">Index.</param>
    public T this[int index]
    {
        get => Buffer[index];
        set => Buffer[index] = value;
    }

    public void Clear()
    {
        Array.Clear(Buffer, 0, Length);
        Length = 0;
    }

    public void Reset()
    {
        Length = 0;
    }

    public int Add(T item)
    {
        if (_freeIndices.Count > 0)
        {
            var idx = _freeIndices.Pop();
            Buffer[idx] = item;
            return idx;
        }
        if (Length == Buffer.Length)
        {
            Array.Resize(ref Buffer, Buffer.Length * 2);
        }
        Buffer[Length] = item;
        return Length++;
    }

    public void RemoveAt(int index)
    {
        Buffer[index] = default;
        _freeIndices.Push(index);
    }
}
