using System.Net.Mime;
using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PolyECS.Systems;
using PolyGame.Components;
using PolyGame.Components.Render;
using PolyGame.Components.Transform;
using PolyGame.Graphics;
using PolyGame.Graphics.Renderable;

namespace PolyGame.Systems.Render;

public class QueueSprites : ClassSystem<(Query, Query)>
{
    // TODO this is ugly. Need an abstraction to generate the queries in the constructor block not the header
    public QueueSprites(World world) : base(
        new BiParam<Query, Query>(new QueryParam(world.Query<ComputedCamera, Managed<RenderableList>>()), new QueryParam(world.Query<Sprite>())), "QueueSprites")
    {
        // TODO GetResource on World
        var registry = world.Get<DrawFuncRegistry>();
        DrawSpriteIndex = registry.RegisterDrawFunc(DrawSprite);
    }

    protected int DrawSpriteIndex;

    public override void Run((Query, Query) param)
    {
        // TODO this is ugly. Should be able to destructure the tuple in the Run method
        var (cameras, sprites) = param;

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

// TODO placeholder
public struct Sprite { }
