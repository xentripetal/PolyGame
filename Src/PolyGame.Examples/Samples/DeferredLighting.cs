using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PolyECS;
using PolyECS.Scheduling.Configs;
using PolyECS.Systems;
using PolyGame.Assets;
using PolyGame.Graphics;
using PolyGame.Graphics.Lights;
using PolyGame.Graphics.Materials;
using PolyGame.Graphics.Renderers;
using PolyGame.Graphics.Sprites;
using PolyGame.Transform;
using Verse;

namespace PolyGame.Examples.Samples;

public partial class DeferredLighting : ISample
{
    public static void Run()
    {
        using var app = new App();
        app.AddPluginBundle(new DefaultPlugins())
            .AddSystems(Schedules.Startup, SystemConfigs.Of(new SpawnCamera(), new Spawn()).Chained())
            .AddSystem<MouseFollow>(Schedules.Update);
        app.Run();
    }

    public record struct MouseFollowTag { }

    public partial class Spawn : AutoSystem
    {
        public void Run(PolyWorld world, AssetServer assets, FinalRenderTarget renderTarget)
        {
            var screen = world.MustGetResource<Screen>();
            var renderGraph = world.Entity("MainCamera").Get<CameraRenderGraph>();
            // TODO hack
            var renderer = new DeferredLightingRenderer(screen, 0, 2, 0);
            //renderer.EnableDebugBufferRender = true;
            renderGraph.Graph.ClearRenderers().AddRenderer(renderer);
            renderTarget.OnSceneBackBufferSizeChanged += renderGraph.Graph.OnSceneBackBufferSizeChanged;

            screen.SetSize(137 * 9, 89 * 9);
            renderTarget.SetDesignResolution(screen, 137 * 9, 89 * 9, FinalRenderTarget.ResolutionPolicy.ShowAllPixelPerfect);
            world.SetResource(new ClearColor(Color.DarkGray));


            var moonTex = assets.Load<Texture2D>("Content/DeferredLighting/moon.png");
            var moonNormal = assets.Load<Texture2D>("Content/DeferredLighting/moonNorm.png", false);
            var orangeTex = assets.Load<Texture2D>("Content/DeferredLighting/orange.png");
            var orangeNormal = assets.Load<Texture2D>("Content/DeferredLighting/orangeNorm.png", false);
            var bg = assets.Load<Texture2D>("Content/DeferredLighting/bg.png");
            var bgNorm = assets.Load<Texture2D>("Content/DeferredLighting/bgNorm.png", false);
            // TODO materials as assets
            var moonMat = new DeferredSpriteMaterial(assets.Get(moonNormal)!);
            var orangeMat = new DeferredSpriteMaterial(assets.Get(orangeNormal)!);
            var bgMat = new DeferredSpriteMaterial(assets.Get(bgNorm)!);

            var moon = new SpriteBundle(moonTex).WithMaterial(material: moonMat).WithTransform(new TransformBundle(new Vector2(100, 400)))
                .Apply(world.Entity("Moon"));
            var orange = new SpriteBundle(orangeTex).WithMaterial(orangeMat).WithTransform(new TransformBundle(screen.Center).WithScale(0.5f)).Apply(world.Entity("Orange"));
            new SpotLightBundle(2).Apply(world.Entity("OrangeLight")).ChildOf(orange);

            var otherOrange = orange.Clone().Set(new Position2D(new Vector2(200, 200)));
            new SpotLightBundle(2).Apply(world.Entity("OtherOrangeLight")).ChildOf(otherOrange);


            new SpriteBundle(bg).WithZIndex(-1).WithMaterial(bgMat).WithTransform(new TransformBundle(screen.Center, 0, new Vector2(9, 9)))
                .Apply(world.Entity("BG"));

            new TransformBundle().Apply(world.Entity("DirLight")).ChildOf(moon).Set(new DirLight(Color.Red)).Set(new SortLayer(2))
                .Add<GlobalZIndex>().Add<ZIndex>();

            new PointLightBundle(2,
                transform: new TransformBundle(position: new Vector2(100, 100)),
                light: new PointLight(radius: 200, intensity: 2, color: new Color(0.8f, 0.8f, 0.9f))
            ).Apply(world.Entity("MouseLight")).Add<MouseFollowTag>();
        }
    }

    public partial class MouseFollow : AutoSystem
    {
        public void Run(TQuery<Position2D, With<MouseFollowTag>> query, in MouseState mouse, in FinalRenderTarget renderTarget)
        {
            var target = renderTarget;
            var state = mouse;
            query.Each((ref Position2D pos) =>
            {
                pos.Value = (state.Position).ToVector2() * target.Scale;
            });
        }
    }
}
