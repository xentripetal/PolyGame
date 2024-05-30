using JetBrains.Annotations;
using Xunit;

namespace PolyGame.Tests;

[TestSubject(typeof(Core))]
public class CoreRenderingTests
{
    public record struct CurrentFrame(int Value);

    /**
    class TestExtractor : IExtractor
    {
        public void Extract(World sourceWorld, World targetWorld)
        {
            sourceWorld.Each((ref CurrentFrame frame) => {
                targetWorld.Entity<CurrentFrame>().Set(frame);
            });
        }
    }

    public void SetupFrameCounter(Core core)
    {
        core.GameWorld.Entity().Set(new CurrentFrame(0));
        core.GameScheduler.AddSystem((Query<CurrentFrame> frameQuery) => {
            frameQuery.Each((ref CurrentFrame frame) => frame.Value++);
        });

        core.RenderScheduler.AddSystem((Query<CurrentFrame> frameQuery) => {
            frameQuery.Each((ref CurrentFrame frame) => RenderFrame = frame);
        });

        core.Extractors.Add(new TestExtractor());
    }

    protected CurrentFrame RenderFrame = new CurrentFrame();

    [Fact]
    public void TestSynchronousRendering()
    {
        var core = new Core();
        core.SynchronousRendering = true;
        SetupFrameCounter(core);
        core.Tick();
        Assert.Equal(1, getSingleton<CurrentFrame>(core.GameWorld).Value);
        Assert.Equal(1, RenderFrame.Value);
    }

    [Fact]
    public void TestAsynchronousRendering()
    {
        var core = new Core();
        core.SynchronousRendering = false;
        SetupFrameCounter(core);
        core.Tick();
        // Output of render should be default
        Assert.Equal(0, RenderFrame.Value);
        Assert.Equal(1, getSingleton<CurrentFrame>(core.GameWorld).Value);
        // We extracted after rendering, so the render world should have the new frame
        Assert.Equal(1, getSingleton<CurrentFrame>(core.RenderWorld).Value);


        core.Tick();
        Assert.Equal(1, RenderFrame.Value);
        Assert.Equal(2, getSingleton<CurrentFrame>(core.GameWorld).Value);
        Assert.Equal(2, getSingleton<CurrentFrame>(core.RenderWorld).Value);
    }
    
    public T getSingleton<T>(World world) where T : struct
    {
        return world.Query<T>().Single<T>();
    }
    **/
}
