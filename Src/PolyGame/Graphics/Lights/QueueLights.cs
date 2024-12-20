using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Assets;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Renderable;
using PolyGame.Graphics.Renderers;
using PolyGame.Graphics.Sprites;
using PolyGame.Transform;

namespace PolyGame.Graphics.Lights;

public abstract partial class CameraQueueBaseSystem : AutoSystem
{
    protected abstract Query CreateRenderableQuery(PolyWorld world);

    [ParamProvider("cameras")]
    protected QueryParam BuildCamerasQuery(PolyWorld world) => Param.Of(world.QueryBuilder().With<ComputedCamera>().In().With<RenderableList>().InOut().Build());
    [ParamProvider("lights")]
    protected QueryParam BuildLightsQuery(PolyWorld world) => Param.Of(CreateRenderableQuery(world));

    [AutoRunMethod]
    public void Run(Query<ComputedCamera, RenderableList> cameras, Query lights)
    {
        cameras.Each((ref ComputedCamera cCam, ref RenderableList renderablesRef) => {
            // can't pass ref to lambda
            var renderables = renderablesRef;
            EvaluateRenderables(cCam, renderables, lights);
        });
    }

    protected abstract void EvaluateRenderables(ComputedCamera cam, RenderableList camRenders, Query renderables);
}

public class QueueSpotLights : CameraQueueBaseSystem
{
    public QueueSpotLights(DrawFuncRegistry registry)
    {
        DrawSpotLightIndex = registry.RegisterDrawFunc(DrawSpotLight);
    }

    protected int DrawSpotLightIndex;

    protected override Query CreateRenderableQuery(PolyWorld world) => world.QueryBuilder().With<GlobalTransform2D>().In().With<SpotLight>().In()
        .With<SortLayer>().In().With<GlobalZIndex>().In().Cached().Build();

    protected override void EvaluateRenderables(ComputedCamera cam, RenderableList camRenders, Query renderables)
    {
        renderables.Each((Entity en, ref GlobalTransform2D transform, ref SpotLight light, ref SortLayer layer, ref GlobalZIndex index) => {
            var scale = transform.Value.Scale;
            var size = light.Radius * scale.X * 2;
            // TODO rotated bounds
            var bounds = new RectangleF(transform.Value.Translation - light.Radius * scale, new Vector2(size, size));
            if (cam.Bounds.Intersects(bounds))
            {
                camRenders.Add(new RenderableReference
                {
                    Entity = en,
                    SortKey = index.Value,
                    SubSortKey = transform.Value.Translation.Y,
                    DrawFuncIndex = DrawSpotLightIndex
                }, layer.Value);
            }
        });
    }

    protected void DrawSpotLight(Renderer renderer, AssetServer assets, RenderableReference renderable, Batcher batch)
    {
        if (renderer is DeferredLightingRenderer deferred)
        {
            deferred.RenderLight(renderable.Entity.Get<GlobalTransform2D>().Value, renderable.Entity.Get<SpotLight>());
        }
    }
}

public class QueueDirLights : CameraQueueBaseSystem
{
    public QueueDirLights(DrawFuncRegistry registry)
    {
        DrawDirLightIndex = registry.RegisterDrawFunc(DrawDirLight);
    }

    protected int DrawDirLightIndex;

    protected override Query CreateRenderableQuery(PolyWorld world)
        => world.QueryBuilder().With<DirLight>().In().With<SortLayer>().In().With<GlobalZIndex>().In().Build();

    protected override void EvaluateRenderables(ComputedCamera cam, RenderableList camRenders, Query renderables)
    {
        renderables.Each((Entity en, ref DirLight light, ref SortLayer layer, ref GlobalZIndex index) => {
            camRenders.Add(new RenderableReference
            {
                Entity = en,
                SortKey = index.Value,
                SubSortKey = 0,
                DrawFuncIndex = DrawDirLightIndex
            }, layer.Value);
        });
    }

    public void DrawDirLight(Renderer renderer, AssetServer assets, RenderableReference renderable, Batcher batch)
    {
        if (renderer is DeferredLightingRenderer deferred)
        {
            deferred.RenderLight(renderable.Entity.Get<DirLight>());
        }
    }
}

public class QueuePointLights : CameraQueueBaseSystem
{
    protected int DrawPointLightIndex;

    public QueuePointLights(DrawFuncRegistry registry)
    {
        DrawPointLightIndex = registry.RegisterDrawFunc(DrawPointLight);
    }

    protected override Query CreateRenderableQuery(PolyWorld world) => world.QueryBuilder().With<GlobalTransform2D>().In().With<PointLight>().In()
        .With<SortLayer>().In().With<GlobalZIndex>().In().Build();

    protected override void EvaluateRenderables(ComputedCamera cam, RenderableList camRenders, Query renderables)
    {
        foreach (var r in renderables)
        {
            
        }
        renderables.Each((Entity en, ref GlobalTransform2D transform, ref PointLight light, ref SortLayer layer, ref GlobalZIndex index) => {
            var scale = transform.Value.Scale;
            var size = light.Radius * scale.X * 2;
            // TODO rotated bounds
            var bounds = new RectangleF(transform.Value.Translation - light.Radius * scale, new Vector2(size, size));
            if (cam.Bounds.Intersects(bounds))
            {
                camRenders.Add(new RenderableReference
                {
                    Entity = en,
                    SortKey = index.Value,
                    SubSortKey = transform.Value.Translation.Y,
                    DrawFuncIndex = DrawPointLightIndex
                }, layer.Value);
            }
        });
    }

    protected void DrawPointLight(Renderer renderer, AssetServer assets, RenderableReference renderable, Batcher batch)
    {
        if (renderer is DeferredLightingRenderer deferred)
        {
            deferred.RenderLight(renderable.Entity.Get<GlobalTransform2D>().Value, renderable.Entity.Get<PointLight>());
        }
    }
}

public class QueueAreaLights : CameraQueueBaseSystem
{
    protected int DrawAreaLightIndex;

    public QueueAreaLights(DrawFuncRegistry registry)
    {
        DrawAreaLightIndex = registry.RegisterDrawFunc(DrawAreaLight);
    }

    protected override Query CreateRenderableQuery(PolyWorld world) => world.QueryBuilder().With<GlobalTransform2D>().In().With<AreaLight>().In()
        .With<SortLayer>().In().With<GlobalZIndex>().In().Build();

    protected override void EvaluateRenderables(ComputedCamera cam, RenderableList camRenders, Query renderables)
    {
        renderables.Each((Entity en, ref GlobalTransform2D transform, ref AreaLight light, ref SortLayer layer, ref GlobalZIndex index) => {
            // TODO rotated bounds
            var bounds = new RectangleF(transform.Value.Translation, new Vector2(light.Width, light.Height) * transform.Value.Scale);
            if (cam.Bounds.Intersects(bounds))
            {
                camRenders.Add(new RenderableReference
                {
                    Entity = en,
                    SortKey = index.Value,
                    SubSortKey = transform.Value.Translation.Y,
                    DrawFuncIndex = DrawAreaLightIndex
                }, layer.Value);
            }
        });
    }

    protected void DrawAreaLight(Renderer renderer, AssetServer assets, RenderableReference renderable, Batcher batch)
    {
        if (renderer is DeferredLightingRenderer deferred)
        {
            deferred.RenderLight(renderable.Entity.Get<GlobalTransform2D>().Value, renderable.Entity.Get<AreaLight>());
        }
    }
}
