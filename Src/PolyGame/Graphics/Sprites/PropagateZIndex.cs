using Flecs.NET.Core;
using PolyECS;
using PolyECS.Systems;

namespace PolyGame.Graphics.Sprites;

public partial class PropagateZIndex : AutoSystem
{
    [ParamProvider("q")]
    protected QueryParam BuildQuery(PolyWorld world) => new(world.QueryBuilder()
        .With<ZIndex>().In().With<GlobalZIndex>().Out()
        .With<GlobalZIndex>().Optional().Parent().Cascade().In()
        .Cached().Build()
    );

    [AutoRunMethod]
    public void Run(Query q)
    {
        q.Run(it => {
            while (it.Next())
            {
                if (!it.Changed())
                {
                    it.Skip();
                }
                var index = it.Field<ZIndex>(0);
                var globalIndex = it.Field<GlobalZIndex>(1);
                var parentIndex = it.Field<GlobalZIndex>(2);
                var hasParent = it.IsSet(2);

                foreach (var i in it)
                {
                    globalIndex[i].Value = index[i].Value;

                    if (hasParent && index[i].Relative)
                    {
                        globalIndex[i].Value = parentIndex[i].Value + index[i].Value;
                    }
                }
            }
        });
    }
}
