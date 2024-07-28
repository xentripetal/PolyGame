using Flecs.NET.Core;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Components;
using PolyGame.Components.Render;
using PolyGame.Components.Transform;
using PolyGame.Graphics.Renderable;

namespace PolyGame.Graphics.Sprites;

/**
 * Example class for what I want a system to look like
 */
public class QueueSprites : ClassSystem<Query, Query, Res<MissingTexture2D>, Res<AssetServer>>
{
    public QueueSprites(DrawFuncRegistry registry)
    {
        DrawSpriteIndex = registry.RegisterDrawFunc(DrawSprite);
    }

    protected int DrawSpriteIndex;

    protected override (ISystemParam<Query>, ISystemParam<Query>, ISystemParam<Res<MissingTexture2D>>, ISystemParam<Res<AssetServer>>) CreateParams(
        PolyWorld world
    )
    {
        var cameraQuery = world.Query<ComputedCamera, Managed<RenderableList>>();
        var renderableQuery = world.QueryBuilder().With<Sprite>().With<Handle<Texture2D>>().With<GlobalPosition2D>().With<GlobalRotation2D>()
            .With<GlobalScale2D>().Build();
        return (Param.Of(cameraQuery), Param.Of(renderableQuery), Param.OfRes<MissingTexture2D>(), Param.OfRes<AssetServer>());
    }

    protected Texture2D? MissingTexture = null;
    protected AssetServer Assets;

    public override void Run(Query cameras, Query sprites, Res<MissingTexture2D> missingTexture, Res<AssetServer> server)
    {
        if (!server.HasValue)
        {
            return;
        }
        Assets = server.Get();
        if (Assets == null)
        {
            return;
        }
        MissingTexture = null;
        // Check the missing texture resource every frame
        if (missingTexture.HasValue)
        {
            var missingTextureHandle = missingTexture.Get().Value;
            var tex = Assets.Get(missingTextureHandle);
            if (tex == null)
            {
                MissingTexture = tex;
            }
        }

        cameras.Each((ref ComputedCamera cCam, ref Managed<RenderableList> renderables) => {
            var rendValue = renderables.Value;
            sprites.Each((Entity en, ref Sprite sprite, ref Position2D pos) => {
                rendValue.Add(new RenderableReference
                {
                    SortKey = pos.Value.Y, // TODO anchor point
                    DrawFuncIndex = DrawSpriteIndex,
                    Entity = en,
                });
            });
        });
    }

    public void DrawSprite(RenderableReference renderable, Batcher batch)
    {
        var sprite = renderable.Entity.Get<Sprite>();
        var imageHandle = renderable.Entity.Get<Handle<Texture2D>>();
        var pos = renderable.Entity.Get<GlobalPosition2D>().Value;
        var rot = renderable.Entity.Get<GlobalRotation2D>().Value;
        var scale = renderable.Entity.Get<GlobalScale2D>().Value;
        // TODO this is an unregistered read of AssetServer. Really should do something better... 
        var image = Assets.Get(imageHandle);
        if (image == null)
        {
            image = MissingTexture;
            if (image == null)
            {
                return; // Missing texture and we don't have a placeholder.
            }
        }
        // TODO sourceRect
        batch.Draw(image, pos, null, sprite.Color, rot, sprite.Anchor, scale, sprite.Effects, 0);
    }
}

public record struct MissingTexture2D(Handle<Texture2D> Value) { }
