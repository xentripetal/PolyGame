using Microsoft.Xna.Framework;
using PolyECS.Scheduling.Configs;
using PolyGame.Components.Transform;

namespace PolyGame.Transform;

/// <summary>
/// Set enum for the systems relating to transform propagation
/// </summary>
public enum TransformSystem
{
    /// <summary>Propagates changes in transform to children's <see cref="GlobalPosition"/></summary>
    TransformPropagate,
}

public class TransformPlugin : IPlugin
{
    public void Apply(App app)
    {
        app.RegisterType<Vector2>().Member<float>("X").Member<float>("Y");
        app.RegisterType<Position2D>().Member<Vector2>("Value");
        app.RegisterType<Rotation2D>().Member<float>("Value");
        app.RegisterType<Scale2D>().Member<Vector2>("Value");

        var syncSystem = new TransformSync();
        app.ConfigureSets(Schedules.PostStartup, SetConfigs.Of(TransformSystem.TransformPropagate))
            .AddSystems(Schedules.PostStartup, syncSystem.InSet(TransformSystem.TransformPropagate))
            .ConfigureSets(Schedules.PostUpdate, SetConfigs.Of(TransformSystem.TransformPropagate))
            .AddSystems(Schedules.PostUpdate, syncSystem.InSet(TransformSystem.TransformPropagate));
    }
}
