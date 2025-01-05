using System;

namespace PolyECS.Tests;

public class WorldFixture : IDisposable
{
    public PolyWorld World { get; } = new();

    public void Dispose()
    {
        World.Dispose();
    }
}