using Microsoft.Xna.Framework;

namespace PolyGame.Components.Transform;

public class TransformPlugin : IPlugin
{
    public void Apply(App app)
    {
        app.World.World.Component<Vector2>().Member<float>("X").Member<float>("Y");
        app.World.World.Component<Position2D>().Member<Vector2>("Value");
        app.World.World.Component<Rotation2D>().Member<float>("Value");
        app.World.World.Component<Scale2D>().Member<Vector2>("Value");
        app.World.World.Component<GlobalPosition2D>().Member<Vector2>("Value");
        app.World.World.Component<GlobalRotation2D>().Member<float>("Value");
        app.World.World.Component<GlobalScale2D>().Member<Vector2>("Value");
        
    }
}
