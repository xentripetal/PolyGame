using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame;
using PolyGame.Graphics.Sprites;

namespace Verse;

public class SpawnTestSprites : ClassSystem<PolyWorld, ResMut<AssetServer>>
{
    protected override (ISystemParam<PolyWorld>, ISystemParam<ResMut<AssetServer>>) CreateParams(PolyWorld world)
        => (Param.OfWorld(), Param.OfResMut<AssetServer>());

    public override void Run(PolyWorld world, ResMut<AssetServer> assets)
    {
        var assetServer = assets.Get();
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                var entity = world.Entity($"Sprite{x}_{y}");
                var sb = new SpriteBundle
                {
                    Texture = assetServer.Load<Texture2D>("Content/Missing.png", true),
                    Transform =
                    {
                        Position = new Vector2(x * 16, y * 16)
                    }
                };
                sb.Apply(entity);
            }
        }
    }
}
