using System.Net.Mime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components;
using PolyGame.Components.Render;
using PolyGame.Components.Transform;
using PolyGame.Graphics;
using PolyGame.Graphics.Renderable;
using TinyEcs;

namespace PolyGame.Systems.Render;

public class QueueSprites
{
    public QueueSprites(Scheduler scheduler)
    {
        var registry = scheduler.GetResource<DrawFuncRegistry>().UnwrappedValue;
        DrawSpriteIndex = registry.RegisterDrawFunc(DrawSprite);

        scheduler.AddSystem((Query<(ComputedCamera, Managed<RenderableList>)> cameras, Query<Sprite> sprites) => {
            Queue(cameras, sprites);
        }, Stages.AfterUpdate);
    }

    protected int DrawSpriteIndex;

    public void Queue(Query<(ComputedCamera, Managed<RenderableList> renderables)> cameras, Query<Sprite> sprites)
    {
        cameras.Each((ref ComputedCamera cCam, ref Managed<RenderableList> renderables) => {
            foreach (var (entities, sprites) in sprites.Iter<Sprite>())
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    renderables.Value.Add(new RenderableReference
                    {
                        SortKey = 0,
                        DrawFuncIndex = DrawSpriteIndex,
                        Entity = entities[i]
                    });
                }
            }
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
