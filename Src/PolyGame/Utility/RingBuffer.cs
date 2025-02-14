using System.Numerics;

namespace PolyGame;

public struct RingBuffer
{
    private readonly float[] values;
    private readonly int length;
    private int tail = 0;
    private int head;

    public RingBuffer(int length)
    {
        values = new float[length];
        this.length = length;
        Min = float.MaxValue;
    }

    public float[] Values => values;

    public int Length => length;

    public int Tail => tail;

    public int Head => head;

    public float Min { get; private set; }
    public float Max { get; private set; }
    public float Avg { get; private set; }

    public void Add(float value)
    {
        var resetMin = false;
        var resetMax = false;
        if (length == 0)
        {
            Avg = value;
            Min = value;
            Max = value;
        }
        else
        {
            resetMin = Math.Abs(values[tail] - Min) < Mathf.Epsilon;
            resetMax = Math.Abs(values[tail] - Max) < Mathf.Epsilon;
            Avg += (value - values[tail]) / length;
            if (Min >= value)
            {
                resetMin = false;
                Min = value;
            }

            if (Max <= value)
            {
                resetMax = false;
                Max = value;
            }
        }


        values[tail] = value;

        tail++;

        if (tail == length)
        {
            tail = 0;
        }

        if (tail < 0)
        {
            tail = length - 1;
        }

        head = (tail - length) % length;

        if (head < 0)
            head += length;

        if (resetMin)
        {
            Min = values[0];
            for (var i = 1; i < length; i++)
                if (values[i] < Min)
                    Min = values[i];
        }
        if (resetMax)
        {
            Max = values[0];
            for (var i = 1; i < length; i++)
                if (values[i] > Max)
                    Max = values[i];
        }
    }
}