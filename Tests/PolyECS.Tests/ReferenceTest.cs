using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Flecs.NET.Core;
using Xunit.Abstractions;

namespace PolyECS.Tests;

// Components
file record struct Position(float X, float Y)
{
    public static Position operator +(Position a, Position b) => new (a.X + b.X, a.Y + b.Y);
    public static Position operator *(Position a, Position b) => new (a.X * b.X, a.Y * b.Y);
}

file record struct Scale(float X, float Y)
{
    public static Scale operator +(Scale a, Scale b) => new (a.X + b.X, a.Y + b.Y);
    public static Scale operator *(Scale a, Scale b) => new (a.X * b.X, a.Y * b.Y);
}

// Tags
file struct Local;
file struct Global;

public class ReferenceTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ReferenceTest(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

    [Fact]
    public void TestWhatIsReference()
    {
        Assert.False(RuntimeHelpers.IsReferenceOrContainsReferences<int>());
    }
    
    public class TestClassComponent
    {
        public List<bool> Value;
    }

    [Fact]
    public void ReferenceClone()
    {
        using var world = World.Create();
        var e1 = world.Entity().Set<TestClassComponent>(new TestClassComponent());
        world.Progress();
        var e2 = e1.Clone();
        world.Progress();
    }


    [Fact]
    public unsafe void TestFlecsTransformQueries()
    {
        using var world = World.Create();
        // Create a hierarchy. For an explanation see the entities/hierarchy example
        var sun = world.Entity("Sun")
            .Add<Position, Global>()
            .Add<Scale, Global>()
            .Set<Position, Local>(new Position(1, 1))
            .Set<Scale, Local>(new Scale(1, 1));

        world.Entity("Mercury")
            .ChildOf(sun)
            .Add<Position, Global>()
            .Add<Scale, Global>()
            .Set<Position, Local>(new Position(1, 1))
            .Set<Scale, Local>(new Scale(1, 1));

        world.Entity("Venus")
            .ChildOf(sun)
            .Add<Position, Global>()
            .Add<Scale, Global>()
            .Set<Position, Local>(new Position(2, 2))
            .Set<Scale, Local>(new Scale(1, 1));

        var earth = world.Entity("Earth")
            .ChildOf(sun)
            .Add<Position, Global>()
            .Add<Scale, Global>()
            .Set<Position, Local>(new Position(3, 3))
            .Set<Scale, Local>(new Scale(2, 2));

        world.Entity("Moon")
            .ChildOf(earth)
            .Add<Position, Global>()
            .Add<Scale, Global>()
            .Set<Position, Local>(new Position(0.1f, 0.1f))
            .Set<Scale, Local>(new Scale(0.1f, 0.1f));

        // Create a hierarchical query to compute the global position from the
        // local position and the parent position.
        using var q = world.QueryBuilder()
            .With<Position, Local>().In() // Self local position
            .With<Scale, Local>().In() // Self local position
            .With<Position, Global>().Out() // Self global position
            .With<Scale, Global>().Out()
            .With<Position, Global>().In().Cascade().Optional()
            .With<Scale, Global>().In().Parent().Optional()
            .Build();
        if (q.Handle == null)
        {
            throw new Exception("Query creation failed");
        }
        q.Each((
            Entity e,
            ref Position localPos,
            ref Scale localScale,
            ref Position globalPos,
            ref Scale globalScale,
            ref Position parentPos,
            ref Scale parentScale
        ) => {
            globalPos = localPos;
            globalScale = localScale;
            _testOutputHelper.WriteLine(e.Name());

            if (!Unsafe.IsNullRef(ref parentPos)) //&& !Unsafe.IsNullRef(parentScale))
            {
                globalPos += parentPos;
                globalScale *= parentScale;
            }
        });

        // Print world positions for all entities that have (Position, Global)
        world.QueryBuilder().With<Position, Global>().With<Scale, Global>().Build().Each((Entity e, ref Position p, ref Scale scale) => {
            _testOutputHelper.WriteLine($"{e.Name()} Position: {p.X}, {p.Y} Scale: {scale.X}, {scale.Y}");
        });
    }
}
