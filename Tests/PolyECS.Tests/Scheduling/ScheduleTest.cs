using JetBrains.Annotations;
using PolyECS.Systems;
using PolyECS.Systems.Configs;

namespace PolyECS.Tests.Scheduling;

[TestSubject(typeof(Schedule))]
public class ScheduleTest
{
    
    [Fact]
    public void FullSchedule()
    {
        /**
         *     fn inserts_a_sync_point() {
        let mut schedule = Schedule::default();
        let mut world = World::default();
        schedule.add_systems(
            (
                |mut commands: Commands| commands.insert_resource(Resource1),
                |_: Res<Resource1>| {},
            )
                .chain(),
        );
        schedule.run(&mut world);

        // inserted a sync point
        assert_eq!(schedule.executable.systems.len(), 3);
    }
         */
        
        var schedule = new Schedule();
        var world = new World();
    }
}
