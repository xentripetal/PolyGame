using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Scheduling.Configs;
using PolyECS.Systems;
using PolyGame.Graphics.Sprites;
using PolyGame.Transform;

namespace PolyGame.Graphics;

[InSet<RenderSets>(RenderSets.ComputeBounds)]
public partial class ComputeSpriteRenderBounds : AutoSystem
{
    public void Run(
        TQuery<RenderBounds, GlobalTransform2D, Sprite, Texture2D, (In<AllTerms>, InOut<Term0>, Cached)> sprites)
    {
        sprites.Query.Iter((Iter it, Field<RenderBounds> bounds, Field<GlobalTransform2D> trans, Field<Sprite> sprites,
            Field<Texture2D> tex) =>
        {
            if (!it.Changed())
            {
                it.Skip();
                return;
            }

            for (int i = 0; i < it.Count(); i++)
            {
                var scale = trans[i].Value.Scale;
                var size = new Vector2(tex[i].Width, tex[i].Height) * scale;
                var pos = trans[i].Value.Translation - sprites[i].Anchor * size;
                bounds[i].Bounds = new RectangleF(pos, size);
            }
        });
    }
}