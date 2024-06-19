using JetBrains.Annotations;
using PolyGame.Components.Render.Extract;
using TinyEcs;

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
                targetWorld.Entity().Set(frame).Set<DeleteAfterRender>();
            });
        }
    }

    public void SetupFrameCounter(Core core)
    {
        core.GameWorld.Entity().Set(new CurrentFrame(0));
        core.GameSchedule.AddSystem((Query<CurrentFrame> query) => {
            query.Each((ref CurrentFrame frame) => {
                frame.Value++;
            });
        });

        core.RenderSchedule.AddSystem((Query<CurrentFrame> query) => {
            query.Each((ref CurrentFrame frame) => {
                RenderFrame = frame;
            });
        });

        core.Extractors.Add(new TestExtractor());
    }

    protected CurrentFrame RenderFrame = new CurrentFrame();
    protected EntityView GameRenderEntity = EntityView.Invalid;

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
        T value = new T();
        world.Query<T>().Each((ref T t) => {
            value = t;
        });
        return value;
    }
}
