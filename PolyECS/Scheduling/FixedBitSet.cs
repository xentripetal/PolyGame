using System.Collections;

namespace PolyFlecs.Systems;

public struct FixedBitSet
{
    public FixedBitSet(int length)
    {
        var capacity = RoundCapacity(length);
        Data = new BitArray(capacity);
        Capacity = capacity;
        Length = length;
    }

    private BitArray Data;
    public int Capacity { get; private set; }
    public int Length { get; private set; }

    /// Returns `true` if `self` has no elements in common with `other`. This
    /// is equivalent to checking for an empty intersection.
    public bool IsDisjoint(FixedBitSet other)
    {
        return new BitArray(Data).And(other.Data).HasAnySet();
    }

    private static int GetInt32ArrayLengthFromBitLength(int n) => n - 1 + 32 >>> 5;

    private static int RoundCapacity(int capacity)
    {
        return (capacity + 31) & ~31;
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity > Capacity)
        {
            var targetCapacity = RoundCapacity(Math.Max(Capacity * 2, capacity));
            var newInts = new int[GetInt32ArrayLengthFromBitLength(targetCapacity)];
            Data.CopyTo(newInts, 0);
            var newData = new BitArray(newInts);
            Data = newData;
            Capacity = Data.Length;
        }
    }

    public void Set(int index)
    {
        SetValue(index, true);
    }

    public void SetValue(int index, bool value)
    {
        EnsureCapacity(index + 1);
        Data.Set(index, value);
        Length = Math.Max(Length, index + 1);
    }

    /// Return **true** if the bit is enabled in the **FixedBitSet**,
    /// **false** otherwise.
    ///
    /// Note: bits outside the capacity are always disabled.
    ///
    /// Note: Also available with index syntax: `bitset[bit]`.
    public bool Contains(int bit)
    {
        return bit < Length && Data.Get(bit);
    }

    public bool Get(int index)
    {
        if (index >= Length)
        {
            throw new IndexOutOfRangeException();
        }
        return Data.Get(index);
    }

    public bool this[int index]
    {
        get => Get(index);
        set => SetValue(index, value);
    }

    public void Clear()
    {
        Data.SetAll(false);
    }

    public void Or(FixedBitSet other)
    {
        if (other.Length > Length)
        {
            EnsureCapacity(other.Capacity);
            Length = other.Length;
        }
        else if (Length > other.Length)
        {
            other.EnsureCapacity(Capacity);
            other.Length = Length;
        }

        Data = Data.Or(other.Data);
    }

    public void And(FixedBitSet other)
    {
        if (other.Length > Length)
        {
            EnsureCapacity(other.Capacity);
            Length = other.Length;
        }
        else if (Length > other.Length)
        {
            other.EnsureCapacity(Capacity);
            other.Length = Length;
        }

        Data = Data.And(other.Data);
    }

    /// Iterates over all enabled bits.
    ///
    /// Iterator element is the index of the `1` bit, type `usize`.
    public IEnumerable<int> Ones()
    {
        for (int i = 0; i < Length; i++)
        {
            if (Data[i])
            {
                yield return i;
            }
        }
    }
}
