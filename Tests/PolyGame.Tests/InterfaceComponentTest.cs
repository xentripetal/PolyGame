using System;
using TinyEcs;
using Xunit.Abstractions;

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

    public InterfaceComponentTest(ITestOutputHelper testOutputHelper)
    {
        _out = testOutputHelper;
    }
    
    [Fact]
    public void Test()
    {
        var world = new World();

        // Create observer for custom event
        world.Entity("T1-Interface").Set(new ITestWrapper(new T1()));
        world.Entity("T2-Interface").Set(new ITestWrapper(new T2()));
        world.Query<ITestWrapper>().Each(en => _out.WriteLine(en.Get<ITestWrapper>().test.DoSomething().ToString()));
        _out.WriteLine("finished");
    }
}
