using System.Runtime.CompilerServices;
using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Assets;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Renderable;
using PolyGame.Transform;

namespace PolyGame.Graphics.Sprites;

/**
 * Example class for what I want a system to look like
 */
public class QueueSprites : ClassSystem<Query, Query, Res<MissingTexture2D>, Res<AssetServer>>
{
    protected int DrawSpriteIndex;

    protected Texture2D? MissingTexture;

    public QueueSprites(DrawFuncRegistry registry) => DrawSpriteIndex = registry.RegisterDrawFunc(DrawSprite);

    protected override (ISystemParam<Query>, ISystemParam<Query>, ISystemParam<Res<MissingTexture2D>>, ISystemParam<Res<AssetServer>>) CreateParams(
        PolyWorld world
    )
    {
        var cameraQuery = world.QueryBuilder().With<ComputedCamera>().In().With<RenderableList>().InOut().Build();
        var renderableQuery = world.QueryBuilder().With<Sprite>().With<GlobalZIndex>().In().With<SortLayer>().With<Handle<Texture2D>>().In().With<GlobalTransform2D>()
            .In().Build();
        return (Param.Of(cameraQuery), Param.Of(renderableQuery), Param.OfRes<MissingTexture2D>(), Param.OfRes<AssetServer>());
    }

    public override void Run(Query cameras, Query sprites, Res<MissingTexture2D> missingTexture, Res<AssetServer> server)
    {
        if (!server.HasValue)
        {
            return;
        }
        MissingTexture = null;
        // Check the missing texture resource every frame
        if (missingTexture.HasValue)
        {
            var missingTextureHandle = missingTexture.Get().Value;
            var tex = server.Get().Get(missingTextureHandle);
            if (tex != null)
            {
                MissingTexture = tex;
            }
        }

        cameras.Each((ref ComputedCamera cCam, ref RenderableList renderablesRef) => {
            // can't pass ref to lambda
            var renderables = renderablesRef;
            var s = server.Get();
            var bounds = cCam.Bounds;
            sprites.Each((
                Entity en,
                ref Sprite sprite,
                ref GlobalZIndex index,
                ref SortLayer layer,
                ref Handle<Texture2D> texHandle,
                ref GlobalTransform2D trans
            ) => {
                var tex = s.Get(texHandle);
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

    public void DrawSprite(AssetServer assets, RenderableReference renderable, Batcher batch)
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

public record struct MissingTexture2D(Handle<Texture2D> Value) { }
