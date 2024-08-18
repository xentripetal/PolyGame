using Flecs.NET.Core;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Assets;
using PolyGame.Transform;

namespace PolyGame.Graphics.Sprites;

public class SpriteExtractor : IExtractor
{
    protected AssetServer AssetServer;

    public SpriteExtractor(AssetServer server) => AssetServer = server;

    public void Extract(World sourceWorld, World targetWorld)
    {
        // TODO multithreaded rendering probably isn't actually needed. Without the controls bevy has to do this optimally this feels like a waste of processing
        // and its not like the rendering is going to be the bottleneck in the first place
        var spriteQuery = sourceWorld.QueryBuilder().With<Sprite>().With<Handle<Texture2D>>().With<GlobalTransform2D>().Build();
        spriteQuery.Each((
            Entity en,
            ref Sprite sprite,
            ref Handle<Texture2D> tex,
            ref GlobalTransform2D transform
        ) => {
            targetWorld.Entity(en.Id)
                .Set(sprite)
                .Set(tex.Clone(AssetServer))
                .Set(transform);
        });

    }
}
