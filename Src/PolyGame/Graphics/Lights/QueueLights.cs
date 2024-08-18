using Flecs.NET.Core;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Renderable;
using PolyGame.Graphics.Sprites;
using PolyGame.Transform;

namespace PolyGame.Graphics.Lights;

public abstract class CameraQueueBaseSystem: ClassSystem<Query, Query> 
{
    protected abstract Query CreateRenderableQuery(PolyWorld world);
    protected override (ISystemParam<Query>, ISystemParam<Query>) CreateParams(
        PolyWorld world
    )
    {
        var cameraQuery = world.QueryBuilder().With<ComputedCamera>().In().With<RenderableList>().InOut().Build();
        return (Param.Of(cameraQuery), Param.Of(CreateRenderableQuery(world)));
    }

    public override void Run(Query cameras, Query lights)
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
    protected override Query CreateRenderableQuery(PolyWorld world) => world.QueryBuilder().With<GlobalTransform2D>().In().With<SpotLight>().In().With<SortLayer>().In().With<GlobalZIndex>().In().Cached().Build();

    protected override void EvaluateRenderables(ComputedCamera cam, RenderableList camRenders, Query renderables)
    {
        renderables.Each((Entity en, ref GlobalTransform2D transform, ref SpotLight light, ref SortLayer layer, ref GlobalZIndex index) => {
            // TODO light bounds
            camRenders.Add(new RenderableReference
            {
                Entity = en,
                SortKey = index.Value,
                SubSortKey = transform.Value.Translation.Y,
                DrawFuncIndex = 0
            }, layer.Value);
        });
    }
}

public class QueueDirLights : CameraQueueBaseSystem
{
    protected override Query CreateRenderableQuery(PolyWorld world) => world.QueryBuilder().With<DirLight>().In().With<SortLayer>().In().With<GlobalZIndex>().In().Build();

    protected override void EvaluateRenderables(ComputedCamera cam, RenderableList camRenders, Query renderables)
    {
        renderables.Each((Entity en, ref DirLight light, ref SortLayer layer, ref GlobalZIndex index) => {
            // TODO light bounds
            camRenders.Add(new RenderableReference
            {
                Entity = en,
                SortKey = index.Value,
                SubSortKey = 0,
                DrawFuncIndex = 0
            }, layer.Value);
        });
    }
}

public class QueuePointLights : CameraQueueBaseSystem
{
    protected override Query CreateRenderableQuery(PolyWorld world) => world.QueryBuilder().With<GlobalTransform2D>().In().With<PointLight>().In().With<SortLayer>().In().With<GlobalZIndex>().In().Build();

    protected override void EvaluateRenderables(ComputedCamera cam, RenderableList camRenders, Query renderables)
    {
        renderables.Each((Entity en, ref GlobalTransform2D transform, ref PointLight light, ref SortLayer layer, ref GlobalZIndex index) => {
            // TODO light bounds
            camRenders.Add(new RenderableReference
            {
                Entity = en,
                SortKey = index.Value,
                SubSortKey = transform.Value.Translation.Y,
                DrawFuncIndex = 0
            }, layer.Value);
        });
    }
}

public class QueueAreaLights : CameraQueueBaseSystem
{
    protected override Query CreateRenderableQuery(PolyWorld world) => world.QueryBuilder().With<GlobalTransform2D>().In().With<AreaLight>().In().With<SortLayer>().In().With<GlobalZIndex>().In().Build();

    protected override void EvaluateRenderables(ComputedCamera cam, RenderableList camRenders, Query renderables)
    {
        renderables.Each((Entity en, ref GlobalTransform2D transform, ref PointLight light, ref SortLayer layer, ref GlobalZIndex index) => {
            // TODO light bounds
            camRenders.Add(new RenderableReference
            {
                Entity = en,
                SortKey = index.Value,
                SubSortKey = transform.Value.Translation.Y,
                DrawFuncIndex = 0
            }, layer.Value);
        });
    }
}