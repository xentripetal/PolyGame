using System.Collections.Generic;
using Flecs.NET.Core;
using JetBrains.Annotations;
using PolyECS.Systems;
using PolyECS.Systems.Configs;

namespace PolyECS.Tests.Scheduling;

[TestSubject(typeof(Schedule))]
public class ScheduleTest 
{
    protected class InsertResourceSys : SimpleSystem
    {
        public int InitCount = 0;
        public int RunCount = 0;
        public override void Initialize(World world)
        {
            InitCount++;
        }

        public override void Run(World world)
        {
            RunCount++;
        }
    }

    [Fact]
    public void EmptySchedule()
    {
        var schedule = new Schedule();
        var world = World.Create();
        schedule.Run(world);
    }
    
    [Fact]
    public void InsertsASyncPoint()
    {
        var schedule = new Schedule();
        var world = World.Create();
        var sysA = new InsertResourceSys();
        var sysB = new InsertResourceSys();
        // Todo add helpers for reducing boilerplate
        schedule.AddSystems(new NodeConfigs<ASystem>.Configs(
            new List<NodeConfigs<ASystem>>(new[]
            {
                NodeConfigs<ASystem>.NewSystem(sysA), NodeConfigs<ASystem>.NewSystem(sysB),
            }),
            new List<Condition>(),
            Chain.Yes)
        );
        schedule.Run(world);
        
        Assert.Equal(3, schedule.Executable.Systems.Count);
        Assert.Equal(1, sysA.InitCount);
        Assert.Equal(1, sysB.InitCount);
        Assert.Equal(1, sysA.RunCount);
        Assert.Equal(1, sysB.RunCount);
    }
}
