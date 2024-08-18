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

public class DeferredLighting : ISample
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

    public class Spawn : ClassSystem<PolyWorld, ResMut<AssetServer>>
    {
        protected override (ISystemParam<PolyWorld>, ISystemParam<ResMut<AssetServer>>) CreateParams(PolyWorld world)
            => (Param.OfWorld(), Param.OfResMut<AssetServer>());

        public override void Run(PolyWorld world, ResMut<AssetServer> resAssets)
        {
            var screen = world.Get<Screen>();
            screen.SetSize(137 * 9, 89 * 9);
            // todo design resolution
            world.SetResource(new ClearColor(Color.DarkGray));

            var renderGraph = world.Entity("MainCamera").Get<CameraRenderGraph>();
            renderGraph.Graph.AddRenderer(new DeferredLightingRenderer(screen, 0, 2, 0));

            var assets = resAssets.Get();
            var moontex = assets.Load<Texture2D>("Content/DeferredLighting/moon.png");
            var moonNormal = assets.Load<Texture2D>("Content/DeferredLighting/moonNorm.png", false);
            var orangeTex = assets.Load<Texture2D>("Content/DeferredLighting/orange.png");
            var orangeNormal = assets.Load<Texture2D>("Content/DeferredLighting/orangeNorm.png", false);
            var bg = assets.Load<Texture2D>("Content/DeferredLighting/bg.png");
            var bgNorm = assets.Load<Texture2D>("Content/DeferredLighting/bgNorm.png", false);
            var moonMat = new DeferredSpriteMaterial(assets.Get(moonNormal)!);
            var orangeMat = new DeferredSpriteMaterial(assets.Get(orangeNormal)!);
            var bgMat = new DeferredSpriteMaterial(assets.Get(bgNorm)!);

            var moon = new SpriteBundle(moontex).WithMaterial(material: moonMat).WithTransform(new TransformBundle(new Vector2(100, 400)))
                .Apply(world.Entity("Moon"));
            var orange = new SpriteBundle(orangeTex).WithMaterial(orangeMat).WithTransform(new TransformBundle(screen.Center)).Apply(world.Entity("Orange"));
            new SpriteBundle(bg).WithZIndex(-1).WithMaterial(bgMat).WithTransform(new TransformBundle(screen.Center, 0, new Vector2(9, 9)))
                .Apply(world.Entity("BG"));

            var dirLight = new TransformBundle().Apply(world.Entity("DirLight")).ChildOf(moon).Set(new DirLight(Color.Red)).Set(new SortLayer(2))
                .Add<GlobalZIndex>().Add<ZIndex>();

            var mouseLight = new TransformBundle(position: new Vector2(100, 100)).Apply(world.Entity("MouseLight"))
                .Set(new PointLight(radius: 200, intensity: 2, color: new Color(0.8f, 0.8f, 0.9f))).Set(new SortLayer(2))
                .Add<GlobalZIndex>().Add<ZIndex>().Add<MouseFollowTag>();
        }
    }

    public class MouseFollow : ClassSystem<Query, Res<MouseState>>
    {
        protected override (ISystemParam<Query>, ISystemParam<Res<MouseState>>) CreateParams(PolyWorld world)
            => (Param.Of(world.Query<Position2D, MouseFollowTag>()), Param.OfRes<MouseState>());

        public override void Run(Query query, Res<MouseState> mouseRes)
        {
            query.Each((ref Position2D pos) => {
                // TODO resolution scaling
                pos.Value = mouseRes.Get().Position.ToVector2();
            });
        }
    }
}
