using Flecs.NET.Core;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components.Transform;

namespace PolyGame.Graphics.Sprites;

public class SpriteExtractor : IExtractor
{
    public SpriteExtractor(AssetServer server)
    {
        AssetServer = server;
    }

    protected AssetServer AssetServer;

    public void Extract(World sourceWorld, World targetWorld)
    {
        // TODO multithreaded rendering probably isn't actually needed. Without the controls bevy has to do this optimally this feels like a waste of processing
        // and its not like the rendering is going to be the bottleneck in the first place
        var spriteQuery = sourceWorld.QueryBuilder().With<Sprite>().With<Handle<Texture2D>>().With<GlobalPosition2D>().With<GlobalRotation2D>()
            .With<GlobalScale2D>().Build();
        spriteQuery.Each(((
                    Entity en,
                    ref Sprite sprite,
                    ref Handle<Texture2D> tex,
                    ref GlobalPosition2D pos,
                    ref GlobalRotation2D rot,
                    ref GlobalPosition2D scale
                ) => {
                    targetWorld.Entity(en.Id)
                        .Set(sprite)
                        .Set(tex.Clone(AssetServer))
                        .Set(pos)
                        .Set(rot)
                        .Set(scale);
                }
            ));

    }
}
