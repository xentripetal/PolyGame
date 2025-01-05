using Flecs.NET.Core;

namespace PolyGame.Tests;

file interface ITest
{
    public bool DoSomething();
}

file record struct ITestWrapper(ITest test);

file class T1 : ITest
{
    public bool DoSomething() => true;
}

file class T2 : ITest
{
    public bool DoSomething() => false;
}

public class InterfaceComponentTest
{
    private readonly ITestOutputHelper _out;

    public InterfaceComponentTest(ITestOutputHelper testOutputHelper) => _out = testOutputHelper;

    [Fact]
    public void Test()
    {
        using var world = World.Create();

        world.Entity("T1-Interface").Set(new ITestWrapper(new T1()));
        world.Entity("T2-Interface").Set(new ITestWrapper(new T2()));
        
        world.Entity("T1-NonStruct").Set<ITest>(new T1());
        world.Entity("T2-NonStruct").Set<ITest>(new T2());
        world.Query<ITest>().Each((Entity entity, ref ITest wrapper) => _out.WriteLine($"{entity.Name()} - {wrapper.DoSomething()}"));
        world.Query<ITestWrapper>().Each((Entity entity, ref ITestWrapper wrapper) => _out.WriteLine($"{entity.Name()} - {wrapper.test.DoSomething()}"));
        _out.WriteLine("finished");
    }
}
