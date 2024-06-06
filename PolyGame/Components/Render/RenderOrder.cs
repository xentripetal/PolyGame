using Flecs.NET.Utilities;

namespace PolyGame.Components.Render;

public struct RenderOrder : IComparable<RenderOrder>
{
    /// <summary>
    /// Order in which this will be rendered. Lower numbers are rendered first.
    /// </summary>
    public uint Order;
    
    /// <summary>
    /// Helper for comparing two RenderOrders in a Flecs query
    /// </summary>
    public static unsafe int Compare(ulong e1, void* p1, ulong e2, void* p2)
    {
        RenderOrder* pos1 = (RenderOrder*)p1;
        RenderOrder* pos2 = (RenderOrder*)p2;
        return pos1->CompareTo(*pos2);
    }

    public int CompareTo(RenderOrder other) => Order.CompareTo(other.Order);
}
