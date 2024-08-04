using System.Runtime.InteropServices;

namespace PolyECS;

/// <summary>Empty placeholder </summary>
public sealed class Empty
{
    private Empty() { }
    public static Empty Instance { get; } = new ();
}
