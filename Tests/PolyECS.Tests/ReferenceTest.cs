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
    public void ReflectionTest()
    {
        using var world = World.Create();
        var e1 = world.Entity().Set<TestClassComponent>(new TestClassComponent());
        var tTypes = e1.Table().Type();
        foreach (var tType in tTypes)
        {
            _testOutputHelper.WriteLine(tType.ToString());
            var componentEntityType = world.Entity(tType).Get<Type>();
            _testOutputHelper.WriteLine(componentEntityType.Name);
        }
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
    public void TableObserver()
    {
        using var world = World.Create();
        world.Observer().With(Ecs.Any).Event(Ecs.OnTableCreate).Each( (Iter it, int i) =>
        {
            _testOutputHelper.WriteLine("Table created");
        });
        world.Entity().Set<int>(1);
    }

    [Fact]
    public void TQueryTest()
    {
        using var world = new PolyWorld();
        world.Entity().Set(1).Set("test").Set(true);
        world.Entity().Set(1).Set("test");


    }

}
