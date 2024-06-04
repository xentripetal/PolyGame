using Flecs.NET.Core;
using JetBrains.Annotations;
using PolyGame.Components.Render.Extract;

namespace PolyGame.Tests;

[TestSubject(typeof(Core))]
public class CoreRenderingTests
{
    public record struct CurrentFrame(int Value);

    class TestExtractor : IExtractor
    {
        public void Extract(World sourceWorld, World targetWorld)
        {
            sourceWorld.Each((ref CurrentFrame frame) => {
                targetWorld.Entity<CurrentFrame>().Set(frame).Add<DeleteAfterRender>();
            });
        }
    }

    public void SetupFrameCounter(Core core)
    {
        GameRenderEntity = core.GameWorld.Entity().Set(new CurrentFrame(0));
        core.GameWorld.Routine<CurrentFrame>().Each((ref CurrentFrame frame) => {
            frame.Value++;
        });

        core.RenderWorld.Routine<CurrentFrame>().Each((ref CurrentFrame frame) => {
            RenderFrame = frame;
        });

        core.Extractors.Add(new TestExtractor());
    }

    protected CurrentFrame RenderFrame = new CurrentFrame();
    protected Entity GameRenderEntity = Entity.Null();

    [Fact]
    public void TestSynchronousRendering()
    {
        var core = new Core();
        core.SynchronousRendering = true;
        SetupFrameCounter(core);
        core.Tick();
        Assert.Equal(1, getSingleton<CurrentFrame>(core.GameWorld).Value);
        Assert.Equal(1, RenderFrame.Value);
        
        core.Tick();
        Assert.Equal(2, getSingleton<CurrentFrame>(core.GameWorld).Value);
        Assert.Equal(2, RenderFrame.Value);
    }

    [Fact]
    public void TestAsynchronousRendering()
    {
        var core = new Core();
        core.SynchronousRendering = false;
        SetupFrameCounter(core);
        for (int i = 0; i < 100; i++)
        {
            core.Tick();
            Assert.Equal(i, RenderFrame.Value);
            Assert.Equal(i + 1, getSingleton<CurrentFrame>(core.GameWorld).Value);
        }
    }
    
    public T getSingleton<T>(World world) where T : struct
    {

        return world.Query<T>().First().Get<T>();
    }
}
