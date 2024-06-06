using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components.Render;
using PolyGame.Components.Transform;

namespace PolyGame.Systems.Render;

public class RendererSystem
{
    protected Query RenderersQ;
    protected GraphicsDevice GraphicsDevice;

    public RendererSystem(World world, GraphicsDevice graphicsDevice)
    {

        GraphicsDevice = graphicsDevice;
        unsafe
        {
            RenderersQ = world.QueryBuilder()
                .With<Camera>()
                .With<RenderTargetConfig>().Optional()
                .With<Material>()
                .With<GlobalTransform>()
                .OrderBy<RenderOrder>(RenderOrder.Compare)
                .Build();
        }

    }

    public void Update(GameTime gameTime)
    {
        RenderersQ.Iter((Iter iter, Field<Camera> camera, Field<RenderTargetConfig> renderTarget, Field<Material> material) => {
            var hasRenderTexture = iter.IsSet(2);
            foreach (int i in iter)
            {
                // TODO need a global/default render texture support like Nez does for scene textures to support "DesignResolution"
                if (hasRenderTexture)
                {
                    GraphicsDevice.SetRenderTarget(renderTarget[i].Texture);
                    GraphicsDevice.Clear(renderTarget[i].ClearColor);
                }
                
            }
        });
    }
}
