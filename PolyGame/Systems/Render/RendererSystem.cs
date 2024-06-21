using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components;
using PolyGame.Components.Render;
using PolyGame.Components.Transform;
using TinyEcs;

namespace PolyGame.Systems.Render;

public class RendererSystem
{
    protected Query RenderersQ;
    protected GraphicsDevice GraphicsDevice;
    protected SpriteBatch batch;
    protected World world;

    public RendererSystem(World world, GraphicsDevice graphicsDevice)
    {
        // TODO batch should be a shared resource
        batch = new SpriteBatch(graphicsDevice, 2048);
        GraphicsDevice = graphicsDevice;
        unsafe
        {
            RenderersQ = world.QueryBuilder()
                .With<Camera>()
                .With<ComputedCamera>()
                .With<CurrentMaterial>()
                .With<PreviousMaterial>()
                .With<GlobalTransform>()
                .With<CameraRenderGraph>()
                .Optional<RenderTargetConfig>()
                .Build();
        }
    }

    public void Update(GameTime gameTime) { }

    protected void Render(Res<ClearColor> clearColor)
    {

        RenderersQ.Each((
            EntityView entity,
            ref Camera cam,
            ref ComputedCamera cCam,
            ref CurrentMaterial curMat,
            ref PreviousMaterial prevMat,
            ref GlobalTransform globalTransform,
            ref CameraRenderGraph renderGraph,
            ref RenderTargetConfig renderTarget
        ) => {
            // TODO need a global/default render texture support like Nez does for scene textures to support "DesignResolution"
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(clearColor.Value.Color);
            var hasRenderTexture = !Unsafe.IsNullRef(renderTarget);
            if (hasRenderTexture)
            {
                GraphicsDevice.SetRenderTarget(renderTarget.Texture);
                GraphicsDevice.Clear(renderTarget.ClearColor);
            }
            batch.Begin(curMat.Material, cCam.TransformMatrix);
            renderGraph.Graph.Render(world, batch, GraphicsDevice);
            batch.End();
        });
    }
}

public record struct ClearColor(Color Color) { };
public record struct CameraRenderGraph(RenderGraph Graph) { };
