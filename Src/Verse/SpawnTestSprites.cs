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
    private Vector2 Offset = new(0, 0);

    [AutoRunMethod]
    public void Run(PolyWorld world, AssetServer assets)
    {
        var missing = assets.TestDirectLoad<Texture2D>("Content/Missing.png");
        for (var x = 0; x < NumCols; x++)
        {
            for (var y = 0; y < NumRows; y++)
            {
                var entity = world.Entity($"Sprite{x}_{y}");
                new SpriteBundle(missing).WithTransform(new TransformBundle(new Vector2(x, y) * 16 + Offset, 0f, Vector2.One)).Apply(entity);
            }
        }
    }
}
