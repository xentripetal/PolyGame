using System.Diagnostics;
using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Assets;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Renderable;
using PolyGame.Graphics.Renderers;
using PolyGame.Transform;
using Serilog;

namespace PolyGame.Graphics.Sprites;

/**
 * Example class for what I want a system to look like
 */
public partial class QueueSprites : AutoSystem
{
    protected int DrawSpriteIndex;
    protected Texture2D? MissingTexture;

    public QueueSprites(DrawFuncRegistry registry) => DrawSpriteIndex = registry.RegisterDrawFunc(DrawSprite);

    public void Run(TQuery<ComputedCamera, RenderableList, In<Term0>> cameras,
        TQuery<RenderBounds, GlobalZIndex, SortLayer, GlobalTransform2D, (In<AllTerms>, With<Sprite>, With<Texture2D>)> sprites,
        [In] MissingTexture2D? missingTexture, AssetServer assets)
    {
        MissingTexture = null;
        // Check the missing texture resource every frame
        if (missingTexture.HasValue)
        {
            var tex = missingTexture.Value.Value.Get(assets);
            if (tex != null)
            {
                MissingTexture = tex;
            }
        }

        cameras.Each((ref ComputedCamera cCam, ref RenderableList renderablesRef) =>
        {
            // can't pass ref to lambda
            var renderables = renderablesRef;
            var bounds = cCam.Bounds;
            sprites.Each((
                Entity en,
                ref RenderBounds spriteBounds,
                ref GlobalZIndex index,
                ref SortLayer layer,
                ref GlobalTransform2D pos
            ) =>
            {
                // TODO doesn't account for rotation
                if (bounds.Intersects(spriteBounds.Bounds))
                {
                    // TODO add metric for num renderables
                    renderables.Add(new RenderableReference
                    {
                        SortKey = index,
                        SubSortKey = pos.Value.Translation.Y,
                        DrawFuncIndex = DrawSpriteIndex,
                        Entity = en
                    }, layer);
                }
            });
            Log.Information("Total Renderables:" + renderables.Count);
        });
    }

    public void DrawSprite(Renderer renderer, AssetServer assets, RenderableReference renderable, Batcher batch)
    {
        var sprite = renderable.Entity.Get<Sprite>();
        var transform = renderable.Entity.Get<GlobalTransform2D>().Value;
        var image = renderable.Entity.Get<Texture2D>();
        if (image == null)
        {
            image = MissingTexture;
            if (image == null)
            {
                return; // Missing texture and we don't have a placeholder.
            }
        }

        // TODO sourceRect
        // TODO transform based draw
        batch.Draw(image, transform.Translation, null, Color.White, transform.RotationDegrees, sprite.Anchor,
            transform.Scale, sprite.Effects, 0);
    }
}

public record struct MissingTexture2D(Handle<Texture2D> Value)
{ }