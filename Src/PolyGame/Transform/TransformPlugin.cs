using Microsoft.Xna.Framework;
using PolyECS.Scheduling.Configs;

namespace PolyGame.Transform;

/// <summary>
///     Set enum for the systems relating to transform propagation
/// </summary>
public enum TransformSystem
{
    /// <summary>Propagates changes in transform to children's <see cref="GlobalPosition" /></summary>
    TransformPropagate
}

public class TransformPlugin : IPlugin
{
    public void Apply(App app)
    {
        app.World.Register<Vector2>().Member<float>("X").Member<float>("Y");
        app.Register<Position2D>().Register<Rotation2D>().Register<Scale2D>();

        var syncSystem = new PropagateTransform();
        app.ConfigureSets(Schedules.PostStartup, SetConfigs.Of(TransformSystem.TransformPropagate))
            .AddSystems(Schedules.PostStartup, syncSystem.InSet(TransformSystem.TransformPropagate))
            .ConfigureSets(Schedules.PostUpdate, SetConfigs.Of(TransformSystem.TransformPropagate))
            .AddSystems(Schedules.PostUpdate, syncSystem.InSet(TransformSystem.TransformPropagate));
    }
}
