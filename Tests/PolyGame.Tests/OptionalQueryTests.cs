using System;
using Flecs.NET.Core;
using Xunit.Abstractions;

namespace PolyGame.Tests;

file record struct Position(float X, float Y);
file record struct Velocity(float X, float Y);

public class OptionalQueryTests 
{
    private readonly ITestOutputHelper _out;

    public OptionalQueryTests(ITestOutputHelper testOutputHelper)
    {
        _out = testOutputHelper;
    }
    
    [Fact]
    public void Test()
    {
        using World world = World.Create();

        // Create observer for custom event
        world.Routine().With<Position>().And().Optional().With<Velocity>().Each(en => _out.WriteLine(en.Name()));
        world.Entity("pos-only").Set(new Position(0, 0));
        world.Entity("pos-and-vel").Set(new Position(0, 0)).Set(new Velocity(1, 1));
        world.Entity("vel-only").Set(new Velocity(1, 1));
        world.Progress();
    }
}
