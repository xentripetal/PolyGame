using System;
using Flecs.NET.Core;
using Xunit.Abstractions;

namespace PolyGame.Tests;

file record struct Position(float X, float Y);
file record struct Velocity(float X, float Y);

public class EventPropagationTest
{
    private readonly ITestOutputHelper _out;

    public EventPropagationTest(ITestOutputHelper testOutputHelper)
    {
        _out = testOutputHelper;
    }
    
    [Fact]
    public void Test()
    {
        using World world = World.Create();

        // Create observer for custom event
        world.Observer()
            .Event(Ecs.MonitorId)
            .Each((Iter it, int i) =>
            {
                if (it.Event() == Ecs.OnAdd)
                    _out.WriteLine($" - Enter: {it.EventId()}: {it.Entity(i)}");
                else if (it.Event() == Ecs.OnRemove)
                    _out.WriteLine($" - Leave: {it.EventId()}: {it.Entity(i)}");
                else
                    _out.WriteLine($" - {it.Event()}: {it.EventId()}: {it.Entity(i)}");
            });

        // Create entity
        Entity e = world.Entity("e");

        // This does not yet trigger the monitor, as the entity does not yet match.
        e.Set<Position>(new(10, 20));

        _out.WriteLine("Before defer begin");
        world.DeferBegin();
        _out.WriteLine("Defer begin");
        // This triggers the monitor with EcsOnAdd, as the entity now matches.
        e.Set<Velocity>(new(1, 2));
        _out.WriteLine("Before defer end");
        world.DeferEnd();
        _out.WriteLine("Defer end");

        // This triggers the monitor with EcsOnRemove, as the entity no longer matches.
        e.Remove<Position>();
    }
}
