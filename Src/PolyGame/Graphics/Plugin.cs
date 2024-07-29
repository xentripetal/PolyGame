using System.Runtime.Serialization.DataContracts;
using Microsoft.Xna.Framework;
using PolyECS;
using PolyECS.Systems;
using PolyECS.Systems.Configs;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Renderable;
using PolyGame.Graphics.Sprites;

namespace PolyGame.Systems.Render;

public class RenderPlugin : IPlugin
{
    public void Apply(App app)
    {
        var registry = new DrawFuncRegistry();
        app.World.World.Component<Matrix2D>().Member<float>("M11").
            Member<float>("M12").
            Member<float>("M21").
            Member<float>("M22").
            Member<float>("M31").
            Member<float>("M32");
        
        app.World.World.Component<RectangleF>().
            Member<float>("X").
            Member<float>("Y").
            Member<float>("Width").
            Member<float>("Height");
        
        app.World.World.Component<Matrix>().
            Member<float>("M11").
            Member<float>("M12").
            Member<float>("M13").
            Member<float>("M14").
            Member<float>("M21").
            Member<float>("M22").
            Member<float>("M23").
            Member<float>("M24").
            Member<float>("M31").
            Member<float>("M32").
            Member<float>("M33").
            Member<float>("M34").
            Member<float>("M41").
            Member<float>("M42").
            Member<float>("M43").
            Member<float>("M44");
        
        app.World.RegisterComponent<ComputedCamera>();
        app.World.RegisterComponent<Camera>();
        app.World.RegisterComponent<CameraInset>();
        
        app.World.SetResource(registry);
        app.World.SetResource(new RenderableList());
        app.World.SetResource(new ClearColor(Microsoft.Xna.Framework.Color.CornflowerBlue));
        app.AddExtractor(new SpriteExtractor(app.World.Get<AssetServer>()));
        app.RenderSchedule.AddSystems(SystemConfigs.Of([new ComputeCameraSystem(), new QueueSprites(registry), new RendererSystem()], chained: Chain.Yes));
    }
}
