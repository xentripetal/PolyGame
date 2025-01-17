using System.Collections;
using PolyECS.Systems;

namespace PolyECS;

public class Events
{ }

public class EventQueue<T> : IEnumerable<T>
{
    public EventQueue(PolyWorld world)
    { }

    /// <summary>
    /// How many <see cref="Update"/>s this queue will persists events for.
    /// </summary>
    public uint TickHistory = 2;

    private T[] _array;
    private uint _start; // The index from which to dequeue if the queue isn't empty.
    private uint _end; // The index at which to enqueue if the queue isn't full.
    private uint _size; // Number of elements.
    private ulong _idx;
    private uint _capacity;

    private Queue<uint> eventOffsetHistory;


    public void Publish(T eventData)
    { }

    public void Update(uint tick)
    { }

    public class Enumerator : IEnumerator<T>
    {
        private int _pos;
        private int _start;
        private int _end;
        private EventQueue<T> _queue;

        public Enumerator(EventQueue<T> queue, int start, int end)
        {
            _queue = queue;
            _end = end;
            _start = start;
            _pos = _start - 1;
        }

        public bool MoveNext()
        {
            if (_pos == _end) return false;
            _pos++;
            if (_pos > _queue._capacity)
            {
                _pos = 0;
            }

            return true;
        }

        public void Reset()
        {
            _pos = _start;
        }

        T IEnumerator<T>.Current => _queue._array[_pos];

        object? IEnumerator.Current => _queue._array[_pos];

        public void Dispose()
        { }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this, (int) _start, (int) _end);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class EventReader<T> : IEnumerable<T>, IStaticSystemParam<EventReader<T>>, IIntoSystemParam
{
    private EventQueue<T> _queue;
    private ulong _idx;

    public EventReader(EventQueue<T> queue)
    {
        _queue = queue;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return null;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static EventReader<T> BuildParamValue(PolyWorld world)
    {
        throw new NotImplementedException();
    }

    public static ISystemParam GetParam(PolyWorld world, EventReader<T> value)
    {
        return value.IntoParam(world);
    }

    public ISystemParam IntoParam(PolyWorld world)
    {
        throw new NotImplementedException();
    }
}

public class EventWriter<T>(EventQueue<T> queue) : IStaticSystemParam<EventWriter<T>>, IIntoSystemParam
{
    public void Publish(T eventData)
    { }

    public static EventWriter<T> BuildParamValue(PolyWorld world)
    {
        throw new NotImplementedException();
    }

    public static ISystemParam GetParam(PolyWorld world, EventWriter<T> value) => value.IntoParam(world);

    public ISystemParam IntoParam(PolyWorld world)
    {
        throw new NotImplementedException();
    }
}