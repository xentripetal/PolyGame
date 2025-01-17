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

public partial class QueueSpotLights : AutoSystem
{
    public override void Initialize(PolyWorld world)
    {
        base.Initialize(world);
        var drawFuncRegistry = world.GetResource<DrawFuncRegistry>().Value;
        if (drawFuncRegistry == null)
            throw new System.Exception("DrawFuncRegistry not found");
        DrawSpotLightIndex = drawFuncRegistry.RegisterDrawFunc(DrawSpotLight);
    }

    protected int DrawSpotLightIndex;

    public void Run(TQuery<ComputedCamera, RenderableList, In<Term0>> cameras, TQuery<GlobalTransform2D, SpotLight, SortLayer, GlobalZIndex, In<AllTerms>> lights)
    {
        cameras.Each((ref ComputedCamera cCam, ref RenderableList renderablesRef) =>
        {
            var renderables = renderablesRef;
            var camBounds = cCam.Bounds;

            // can't pass ref to lambda
            lights.Each((Entity en, ref GlobalTransform2D transform, ref SpotLight light, ref SortLayer layer,
                ref GlobalZIndex index) =>
            {
                var scale = transform.Value.Scale;
                var size = light.Radius * scale.X * 2;
                // TODO rotated bounds
                var bounds = new RectangleF(transform.Value.Translation - light.Radius * scale, new Vector2(size, size));
                if (camBounds.Intersects(bounds))
                {
                    renderables.Add(new RenderableReference
                    {
                        Entity = en,
                        SortKey = index.Value,
                        SubSortKey = transform.Value.Translation.Y,
                        DrawFuncIndex = DrawSpotLightIndex
                    }, layer.Value);
                }
            });
        });
    }


    public static void DrawSpotLight(Renderer renderer, AssetServer assets, RenderableReference renderable, Batcher batch)
    {
        if (renderer is DeferredLightingRenderer deferred)
        {
            deferred.RenderLight(renderable.Entity.Get<GlobalTransform2D>().Value, renderable.Entity.Get<SpotLight>());
        }
    }
}

public partial class QueueDirLights : AutoSystem
{

    public override void Initialize(PolyWorld world)
    {
        base.Initialize(world);
        DrawDirLightIndex = world.MustGetResource<DrawFuncRegistry>().RegisterDrawFunc(DrawDirLight);
    }

    protected int DrawDirLightIndex;


    public void Run(TQuery<ComputedCamera, RenderableList, In<Term0>> cameras, TQuery<DirLight, SortLayer, GlobalZIndex, In<AllTerms>> lights)
    {
        cameras.Each((ref ComputedCamera cCam, ref RenderableList renderablesRef) =>
        {
            var renderables = renderablesRef;
            // can't pass ref to lambda
            lights.Each((Entity en, ref DirLight lights, ref SortLayer layer, ref GlobalZIndex index) =>
            {
                renderables.Add(new RenderableReference
                {
                    Entity = en,
                    SortKey = index.Value,
                    SubSortKey = 0,
                    DrawFuncIndex = DrawDirLightIndex
                }, layer.Value);
            });
        });
    }

    public static void DrawDirLight(Renderer renderer, AssetServer assets, RenderableReference renderable, Batcher batch)
    {
        if (renderer is DeferredLightingRenderer deferred)
        {
            deferred.RenderLight(renderable.Entity.Get<DirLight>());
        }
    }
}

public partial class QueuePointLights : AutoSystem
{
    protected int DrawPointLightIndex;

    public override void Initialize(PolyWorld world)
    {
        base.Initialize(world);
        DrawPointLightIndex = world.MustGetResource<DrawFuncRegistry>().RegisterDrawFunc(DrawPointLight);
    }

    public void Run(TQuery<ComputedCamera, RenderableList, In<Term0>> cameras, TQuery<GlobalTransform2D, PointLight, SortLayer, GlobalZIndex, In<AllTerms>> lights)
    {
        cameras.Each((ref ComputedCamera cCam, ref RenderableList renderablesRef) =>
        {
            var renderables = renderablesRef;
            var camBounds = cCam.Bounds;

            // can't pass ref to lambda
            lights.Each((Entity en, ref GlobalTransform2D transform, ref PointLight light, ref SortLayer layer, ref GlobalZIndex index) =>
            {
                var scale = transform.Value.Scale;
                var size = light.Radius * scale.X * 2;
                // TODO rotated bounds
                var bounds = new RectangleF(transform.Value.Translation - light.Radius * scale, new Vector2(size, size));
                if (camBounds.Intersects(bounds))
                {
                    renderables.Add(new RenderableReference
                    {
                        Entity = en,
                        SortKey = index.Value,
                        SubSortKey = transform.Value.Translation.Y,
                        DrawFuncIndex = DrawPointLightIndex
                    }, layer.Value);
                }
            });
        });
    }

    protected static void DrawPointLight(Renderer renderer, AssetServer assets, RenderableReference renderable, Batcher batch)
    {
        if (renderer is DeferredLightingRenderer deferred)
        {
            deferred.RenderLight(renderable.Entity.Get<GlobalTransform2D>().Value, renderable.Entity.Get<PointLight>());
        }
    }
}

public partial class QueueAreaLights : AutoSystem
{
    protected int DrawAreaLightIndex;

    public override void Initialize(PolyWorld world)
    {
        base.Initialize(world);
        DrawAreaLightIndex = world.MustGetResource<DrawFuncRegistry>().RegisterDrawFunc(DrawAreaLight);
    }

    public void Run(TQuery<ComputedCamera, RenderableList, In<Term0>> cameras, TQuery<GlobalTransform2D, AreaLight, SortLayer, GlobalZIndex, In<AllTerms>> lights)
    {
        cameras.Each((ref ComputedCamera cCam, ref RenderableList renderablesRef) =>
        {
            var renderables = renderablesRef;
            var camBounds = cCam.Bounds;

            lights.Each((Entity en, ref GlobalTransform2D transform, ref AreaLight light, ref SortLayer layer,
                ref GlobalZIndex index) =>
            {
                // TODO rotated bounds
                var bounds = new RectangleF(transform.Value.Translation,
                    new Vector2(light.Width, light.Height) * transform.Value.Scale);
                if (camBounds.Intersects(bounds))
                {
                    renderables.Add(new RenderableReference
                    {
                        Entity = en,
                        SortKey = index.Value,
                        SubSortKey = transform.Value.Translation.Y,
                        DrawFuncIndex = DrawAreaLightIndex
                    }, layer.Value);
                }
            });
        });
    }

    public static void DrawAreaLight(Renderer renderer, AssetServer assets, RenderableReference renderable, Batcher batch)
    {
        if (renderer is DeferredLightingRenderer deferred)
        {
            deferred.RenderLight(renderable.Entity.Get<GlobalTransform2D>().Value, renderable.Entity.Get<AreaLight>());
        }
    }
}