using PolyECS;
using PolyECS.Systems;
using PolyGame.Graphics.Renderable;

namespace PolyGame.Systems.Render;

public class RenderPlugin 
{
    public void Build(PolyWorld world, Schedule schedule)
    {
        world.AddResource(new RenderableList());
        world.AddResource(new ClearColor(Microsoft.Xna.Framework.Color.CornflowerBlue));
        RendererSystem renderer = new (world, schedule);
    }
}
