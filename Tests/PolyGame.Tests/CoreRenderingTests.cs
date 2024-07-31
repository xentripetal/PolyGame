using Flecs.NET.Core;
using JetBrains.Annotations;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Components.Render.Extract;

namespace PolyGame.Tests;

[TestSubject(typeof(App))]
public class CoreRenderingTests
{
    public record struct CurrentFrame(int Value);

    class TestExtractor : IExtractor
    {
        public void Extract(World sourceWorld, World targetWorld)
        {
            sourceWorld.Each((ref CurrentFrame frame) => {
                targetWorld.Entity().Set(frame).Add<DeleteAfterRender>();
            });
        }
    }
    
    protected class FrameIncSystem : ClassSystem<Query>
    {
        protected override ISystemParam<Query> CreateParam(PolyWorld world) {
            return Param.Of(world.Query<CurrentFrame>());
        }

        public override void Run(Query param)
        {
            param.Each((ref CurrentFrame frame) => {
                frame.Value++;
            });
        }
    }

    public class FrameTracker: ClassSystem<Query>
    {
        public CurrentFrame RenderFrame = new CurrentFrame(0);

        protected override ISystemParam<Query> CreateParam(PolyWorld world)
        {
            return Param.Of(world.Query<CurrentFrame>());
        }

        public override void Run(Query param)
        {
            param.Each((ref CurrentFrame frame) => {
                RenderFrame = frame;
            });
        }
    }

    public FrameTracker SetupFrameCounter(App app)
    {
        app.World.Entity().Set(new CurrentFrame(0));
        app.GameSchedule.AddSystems(new FrameIncSystem());
        var tracker = new FrameTracker();
        app.RenderSchedule.AddSystems(tracker);
        return tracker;
    }

    [Fact]
    public void TestSynchronousRendering()
    {
        var core = new App();
        var tracker = SetupFrameCounter(core);
        core.Tick();
        Assert.Equal(1, getSingleton<CurrentFrame>(core.World).Value);
        Assert.Equal(1, tracker.RenderFrame.Value);

        core.Tick();
        Assert.Equal(2, getSingleton<CurrentFrame>(core.World).Value);
        Assert.Equal(2, tracker.RenderFrame.Value);
    }

    /** Disabling as I've stopped async rendering for now. Will come back to it later maybe
    [Fact]
    public void TestAsynchronousRendering()
    {
        var core = new App();
        core.SynchronousRendering = false;
        var tracker = SetupFrameCounter(core);
        for (int i = 0; i < 100; i++)
        {
            core.Tick();
            Assert.Equal(i, tracker.RenderFrame.Value);
            Assert.Equal(i + 1, getSingleton<CurrentFrame>(core.World).Value);
        }
    }
    **/

    public T getSingleton<T>(PolyWorld world) where T : struct
    {
        T value = new T();
        world.Query<T>().Each((ref T t) => {
            value = t;
        });
        return value;
    }
}
