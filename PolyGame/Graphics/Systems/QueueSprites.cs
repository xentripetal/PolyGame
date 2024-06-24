using System.Net.Mime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components;
using PolyGame.Components.Render;
using PolyGame.Graphics.Renderable;
using TinyEcs;

namespace PolyGame.Systems.Render;

public class QueueSprites
{
    public QueueSprites(Scheduler scheduler)
    {
        var registry = scheduler.GetResource<DrawFuncRegistry>().UnwrappedValue;
        //DrawSpriteIndex = registry.RegisterDrawFunc(DrawSprite);

        scheduler.AddSystem((Query<(ComputedCamera, Managed<RenderableList>)> cameras, Query<Sprite> sprites) => {
            Queue(cameras, sprites);
        }, Stages.AfterUpdate);
        tempTexture = scheduler.GetResource<ContentManager>().UnwrappedValue.Load<Texture2D>("MissingTexture");
    }

    protected int DrawSpriteIndex;
    protected Texture2D tempTexture;

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

    public void DrawSprite(RenderableReference renderable, SpriteBatch batch)
    {
        var sprite = renderable.Entity.Get<Sprite>();
        batch.Draw(tempTexture, Vector2.Zero, Color.White);
    }
}

// TODO placeholder
public struct Sprite { }
