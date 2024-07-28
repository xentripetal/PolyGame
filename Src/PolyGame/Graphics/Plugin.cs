using PolyECS;
using PolyECS.Systems;
using PolyECS.Systems.Configs;
using PolyGame.Graphics.Renderable;
using PolyGame.Graphics.Sprites;

namespace PolyGame.Systems.Render;

public class RenderPlugin : IPlugin
{
    public void Apply(App app)
    {
        var registry = new DrawFuncRegistry();
        app.RenderWorld.SetResource(registry);
        app.RenderWorld.SetResource(new RenderableList());
        app.RenderWorld.SetResource(new ClearColor(Microsoft.Xna.Framework.Color.CornflowerBlue));
        app.AddExtractor(new SpriteExtractor(app.RenderWorld.Get<AssetServer>()));
        RendererSystem renderer = new ();
        QueueSprites queueSprites = new (registry);
        app.RenderSchedule.AddSystems(SystemConfigs.Of([queueSprites, renderer], chained: Chain.Yes));
    }
}
