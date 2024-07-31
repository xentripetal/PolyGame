using System.Runtime.CompilerServices;

namespace PolyGame;

public class ListPool<T>
{
    protected T[] Buffer;
    protected ushort[] Generations;
    protected Stack<int> _freeIndices = new Stack<int>();

    protected int Length = 0;

    public ListPool(int capacity)
    {
        Buffer = new T[capacity];
        Generations = new ushort[capacity];
    }

    public int Count => Length - _freeIndices.Count;
    public int Capacity => Buffer.Length;

    public ListPool() : this(5) { }

    public T this[int index]
    {
        get => Buffer[index];
        set => Buffer[index] = value;
    }

    public void Clear()
    {
        Array.Clear(Buffer, 0, Length);
        Array.Clear(Generations, 0, Length);
        _freeIndices.Clear();
        Length = 0;
    }

    public void Reset()
    {
        Length = 0;
        _freeIndices.Clear();
    }

    public (int, ushort) Add(T item)
    {
        if (_freeIndices.Count > 0)
        {
            var idx = _freeIndices.Pop();
            Buffer[idx] = item;
            Generations[idx]++;
            if (Generations[idx] == 0)
                Generations[idx]++;
            return (idx, Generations[idx]);
        }
        if (Length == Buffer.Length)
        {
            Array.Resize(ref Buffer, Buffer.Length * 2);
            Array.Resize(ref Generations, Generations.Length * 2);
        }
        Buffer[Length] = item;
        var gen = Generations[Length];
        gen++;
        if (gen == 0)
            gen++;
        Generations[Length] = gen;
        return (Length++, gen);
    }

    public ushort GetGeneration(int index)
    {
        if (index < 0 || index >= Length)
            return 0;
        return Generations[index];
    }

    public void Free(int index)
    {
        _freeIndices.Push(index);
    }
}
