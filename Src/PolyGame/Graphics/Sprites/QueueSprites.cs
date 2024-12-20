using System.Runtime.CompilerServices;
using DotNext;
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

namespace PolyGame.Graphics.Sprites;

/**
 * Example class for what I want a system to look like
 */
public partial class QueueSprites : AutoSystem 
{
    protected int DrawSpriteIndex;
    protected Texture2D? MissingTexture;

    public QueueSprites(DrawFuncRegistry registry) => DrawSpriteIndex = registry.RegisterDrawFunc(DrawSprite);

    [ParamProvider("cameras")]
    public QueryParam BuildCamerasQuery(PolyWorld world)
    {
        return Param.Of(world.QueryBuilder().With<ComputedCamera>().In().With<RenderableList>().InOut().Build());
    }
    
    [ParamProvider("sprites")]
    public QueryParam BuildRenderableQuery(PolyWorld world)
    {
        return Param.Of(world.QueryBuilder().With<Sprite>().With<GlobalZIndex>().In().With<SortLayer>().With<Handle<Texture2D>>().In().With<GlobalTransform2D>().In().Build());
    }

    [AutoRunMethod]
    public void Run(Query<ComputedCamera, RenderableList> cameras, Query<Sprite, GlobalZIndex, SortLayer, Handle<Texture2D>, GlobalTransform2D> sprites, MissingTexture2D? missingTexture, AssetServer assets)
    {
        MissingTexture = null;
        // Check the missing texture resource every frame
        if (missingTexture.HasValue)
        {
            var tex = assets.Get(missingTexture.Value.Value);
            if (tex != null)
            {
                MissingTexture = tex;
            }
        }

        cameras.Each((ref ComputedCamera cCam, ref RenderableList renderablesRef) => {
            // can't pass ref to lambda
            var renderables = renderablesRef;
            var bounds = cCam.Bounds;
            sprites.Each((
                Entity en,
                ref Sprite sprite,
                ref GlobalZIndex index,
                ref SortLayer layer,
                ref Handle<Texture2D> texHandle,
                ref GlobalTransform2D trans
            ) => {
                var tex = assets.Get(texHandle);
                if (tex == null)
                {
                    if (MissingTexture == null)
                    {
                        return;
                    }
                    tex = MissingTexture;
                }

                var scale = trans.Value.Scale;
                var size = new Vector2(tex.Width * scale.X, tex.Height * scale.Y);
                var pos = trans.Value.Translation - sprite.Anchor * size;

                // TODO doesn't account for rotation
                if (bounds.Intersects(new RectangleF(pos, size)))
                {
                    // TODO add metric for num renderables
                    renderables.Add(new RenderableReference
                    {
                        SortKey = index,
                        SubSortKey = pos.Y,
                        DrawFuncIndex = DrawSpriteIndex,
                        Entity = en
                    }, layer);
                }
            });
        });
    }

    public void DrawSprite(Renderer renderer, AssetServer assets, RenderableReference renderable, Batcher batch)
    {
        var sprite = renderable.Entity.Get<Sprite>();
        var imageHandle = renderable.Entity.Get<Handle<Texture2D>>();
        var transform = renderable.Entity.Get<GlobalTransform2D>().Value;
        var image = assets.Get(imageHandle);
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
        batch.Draw(image, transform.Translation, null, Color.White, transform.RotationDegrees, sprite.Anchor, transform.Scale, sprite.Effects, 0);
    }
}

public class QueryAnnotationAttribute : Attribute
{
    public QueryAnnotationAttribute(string empty)
    {
        throw new NotImplementedException();
    }
}

public record struct MissingTexture2D(Handle<Texture2D> Value) { }
