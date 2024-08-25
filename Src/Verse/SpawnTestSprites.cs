using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Assets;
using PolyGame.Graphics.Sprites;
using PolyGame.Transform;

namespace Verse;

public class SpawnTestSprites : ClassSystem<PolyWorld, ResMut<AssetServer>>
{
    private const int NumCols = 1;
    private const int NumRows = 1;

    protected override (ISystemParam<PolyWorld>, ISystemParam<ResMut<AssetServer>>) CreateParams(PolyWorld world)
        => (Param.OfWorld(), Param.OfResMut<AssetServer>());


    public override void Run(PolyWorld world, ResMut<AssetServer> assets)
    {
        var assetServer = assets.Get();
        for (var x = 0; x < NumCols; x++)
        {
            for (var y = 0; y < NumRows; y++)
            {
                var entity = world.Entity($"Sprite{x}_{y}");
                new SpriteBundle(assetServer.Load<Texture2D>("Content/Missing.png")).WithTransform(new TransformBundle(new Vector2(x, y) * 16)).Apply(entity);
            }
        }
    }
}
