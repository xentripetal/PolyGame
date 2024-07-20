using PolyECS;
using PolyECS.Systems;
using PolyECS.Systems.Configs;
using PolyGame.Graphics.Renderable;

namespace PolyGame.Systems.Render;

public class RenderPlugin
{
    public void Build(PolyWorld world, Schedule schedule)
    {
        world.SetResource(new RenderableList());
        world.SetResource(new ClearColor(Microsoft.Xna.Framework.Color.CornflowerBlue));
        RendererSystem renderer = new ();
        QueueSprites queueSprites = new (world);
        schedule.AddSystems(SystemConfigs.Of([queueSprites, renderer], chained: Chain.Yes));
    }
}
