using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Components;
using PolyGame.Components.Render;
using PolyGame.Components.Transform;
using PolyGame.Graphics;
using PolyGame.Graphics.Renderable;

namespace PolyGame.Systems.Render;

/**
 * Example class for what I want a system to look like
 */
public class QueueSprites : ClassSystem<Query, Query>
{
    public QueueSprites(PolyWorld world)
    {
        var registry = world.World.Get<DrawFuncRegistry>();
        DrawSpriteIndex = registry.RegisterDrawFunc(DrawSprite);
    }

    protected override (ISystemParam<Query>, ISystemParam<Query>) CreateParams(PolyWorld world)
    {
        var cameraQuery = world.World.Query<ComputedCamera, Managed<RenderableList>>();
        var renderableQuery = world.World.Query<Sprite>();
        return (Param.Of(cameraQuery), Param.Of(renderableQuery));
    }

    protected int DrawSpriteIndex;

    public override void Run(Query cameras, Query sprites)
    {
        cameras.Each((ref ComputedCamera cCam, ref Managed<RenderableList> renderables) => {
            var rendValue = renderables.Value;
            sprites.Each((Entity en, ref Sprite sprite) => {
                rendValue.Add(new RenderableReference
                {
                    SortKey = 0,
                    DrawFuncIndex = DrawSpriteIndex,
                    Entity = en,
                });
            });
        });
    }

    public void DrawSprite(RenderableReference renderable, Batcher batch)
    {
        var sprite = renderable.Entity.Get<Sprite>();
        var pos = renderable.Entity.Get<GlobalPosition2D>().Value;
        var rot = renderable.Entity.Get<GlobalRotation2D>().Value;
        var scale = renderable.Entity.Get<GlobalScale2D>().Value;

        var texture = renderable.Entity.Get<Managed<Texture2D>>();
        batch.Draw(texture.Value, pos, null, Color.White, rot, Vector2.Zero, scale, SpriteEffects.None, 0);
    }
}

/// <summary>
/// Placeholder sprite 
/// </summary>
public struct Sprite
{
    
}