using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Assets;
using PolyGame.Graphics.Sprites;
using PolyGame.Transform;

namespace Verse;

public partial class SpawnTestSprites : AutoSystem
{
    private const int NumCols = 1;
    private const int NumRows = 1;

    [AutoRunMethod]
    public void Run(PolyWorld world, AssetServer assets)
    {
        for (var x = 0; x < NumCols; x++)
        {
            for (var y = 0; y < NumRows; y++)
            {
                var entity = world.Entity($"Sprite{x}_{y}");
                new SpriteBundle(assets.Load<Texture2D>("Content/Missing.png")).WithTransform(new TransformBundle(new Vector2(x, y) * 16)).Apply(entity);
            }
        }
    }
}
