using Flecs.NET.Core;
using JetBrains.Annotations;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Components.Render.Extract;
using World = Flecs.NET.Core.World;

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
                targetWorld.Entity().Set(frame).Add<DeleteAfterRender>();
            });
        }
    }
    
    protected class FrameIncSystem : ClassSystem<Query>
    {
        protected override ISystemParam<Query> CreateParam(PolyWorld world) {
            return Param.Of(world.World.Query<CurrentFrame>());
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
            return Param.Of(world.World.Query<CurrentFrame>());
        }

        public override void Run(Query param)
        {
            param.Each((ref CurrentFrame frame) => {
                RenderFrame = frame;
            });
        }
    }

    public FrameTracker SetupFrameCounter(Core core)
    {
        core.GameWorld.Entity().Set(new CurrentFrame(0));
        core.GameSchedule.AddSystems(new FrameIncSystem());
        var tracker = new FrameTracker();
        core.RenderSchedule.AddSystems(tracker);
        core.Extractors.Add(new TestExtractor());
        return tracker;
    }

    [Fact]
    public void TestSynchronousRendering()
    {
        var core = new Core();
        core.SynchronousRendering = true;
        var tracker = SetupFrameCounter(core);
        core.Tick();
        Assert.Equal(1, getSingleton<CurrentFrame>(core.GameWorld).Value);
        Assert.Equal(1, tracker.RenderFrame.Value);

        core.Tick();
        Assert.Equal(2, getSingleton<CurrentFrame>(core.GameWorld).Value);
        Assert.Equal(2, tracker.RenderFrame.Value);
    }

    [Fact]
    public void TestAsynchronousRendering()
    {
        var core = new Core();
        core.SynchronousRendering = false;
        var tracker = SetupFrameCounter(core);
        for (int i = 0; i < 100; i++)
        {
            core.Tick();
            Assert.Equal(i, tracker.RenderFrame.Value);
            Assert.Equal(i + 1, getSingleton<CurrentFrame>(core.GameWorld).Value);
        }
    }

    public T getSingleton<T>(PolyWorld world) where T : struct
    {
        T value = new T();
        world.World.Query<T>().Each((ref T t) => {
            value = t;
        });
        return value;
    }
}
